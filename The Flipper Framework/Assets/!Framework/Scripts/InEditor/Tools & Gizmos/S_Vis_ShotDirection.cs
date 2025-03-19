using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[ExecuteInEditMode]
public class S_Vis_ShotDirection : S_Vis_Base
{
#if UNITY_EDITOR
	public S_Vis_ShotDirection () {
		_hasVisualisationScripted = true;
		_drawIfParentSelected = true;

		_selectedFillColour = Color.red;
		_normalOutlineColour = Color.red;
		_normalOutlineColour.a = 0.5f;
		_selectedFillColour = Color.red * 0.5f;
		_selectedFillColour.a = 1;
	}

	[Header("Gizmo")]
	[Tooltip("If true, creates a series of gizmos that show the trajectory of the player after being launched by this. The red lines are the path and positions every frame. The blue line represents when the control lock ends.")]
	public bool	_debugForce;
	[Range(0f, 150f), Tooltip("This is how many physics checks this gizmo should make. This means how many FixedUpdates would be called for the player when calculating their movement after the bounce.")]
	public int	_calculations = 1;

	[Header("Launcher")]
	[Tooltip("The script that causes the player to be launched, or holds the data of the launch.")]
	public MonoBehaviour _LauncherScript;
	[Tooltip("The name of the struc varaible the above script uses to hold the data for the launch.")]
	public string _launcherStructName;


	[Header("Physics")]
	[Tooltip("Put is some character stats and the gizmo will show the trajectory that character would take. Including gravity and deceleration differences.")]
	public S_O_CharacterStats _CharacterStatsToFollow;
	[Tooltip("Where to originate the gizmo from. Can just be the transform of this same object.")]
	public Transform	_ShotCenter;

	private Vector3	_fallGravity = new Vector3(0, -1.5f, 0);
	private Vector3	_upGravity = new Vector3(0, -1.7f, 0);
	private float       _maxFall;
	private float	_constantDeceleration;
	private float	_acceleration;
	private float       _accellModInAir;
	private AnimationCurve _AcellBySpeed;
	private float        _maxSpeed;

	private int         _lockFrames;

	[Header("Simulated")]
	[CustomReadOnly]
	public Vector3 _coreVelAtEnd;
	[CustomReadOnly]
	public Vector3 _environmentalVelAtStart;

	private Vector3[] _debugTrajectoryPoints;

	public override void DrawGizmosAndHandles ( bool selected ) {
		DrawShot(selected);
	}


	private void OnValidate () {
		SetUpShot();
	}

	[ExecuteInEditMode]
	void Update () {
		if(Application.isPlaying) { return; }
		if(_isSelected)
			SetUpShot();
	}

	private void SetUpShot () {
		GetValuesFromScriptableObject();
		//This array will make points along a line following the path the player should take.
		_debugTrajectoryPoints = SetUpLauncherSimulation();
	}

	private void DrawShot ( bool selected ) {
		if (!_LauncherScript) { return; }

		if (_debugForce && _calculations > 0) //Will only show line if there's a line to create.
		{
			DrawGizmosFromArray(_debugTrajectoryPoints, selected);
		}
	}

	private void GetValuesFromScriptableObject () {
		if (!_CharacterStatsToFollow) { enabled = false; return; }

		//Gets values for simulation from stats object.
		_constantDeceleration = _CharacterStatsToFollow.DecelerationStats.airConstantDecel;
		_fallGravity = _CharacterStatsToFollow.WhenInAir.fallGravity;
		_upGravity = _CharacterStatsToFollow.WhenInAir.upGravity;
		_maxFall = _CharacterStatsToFollow.WhenInAir.startMaxFallingSpeed;
		_acceleration = _CharacterStatsToFollow.AccelerationStats.runAcceleration;
		_accellModInAir = _CharacterStatsToFollow.WhenInAir.controlAmmount.y;
		_AcellBySpeed = _CharacterStatsToFollow.AccelerationStats.AccelBySpeed;
		_maxSpeed = _CharacterStatsToFollow.SpeedStats.maxSpeed;
	}


	private Vector3[] SetUpLauncherSimulation () {

		string translatedVariableName = S_S_Editor.TranslateStringToVariableName(_launcherStructName, S_EditorEnums.CasingTypes.camelCase);
		object value = S_S_Editor.FindFieldByName(_LauncherScript, translatedVariableName, "").value;

		if (value == null || value.GetType() != typeof(LaunchPlayerData)) { return null; }

		_launcherStructName = translatedVariableName;

		LaunchPlayerData launcherData = (LaunchPlayerData) value;

		if (launcherData._overwriteGravity_ != Vector3.zero)
		{
			_fallGravity = launcherData._overwriteGravity_;
			_upGravity = launcherData._overwriteGravity_;
		}
		_lockFrames = launcherData._lockInputFrames_;
		Vector3 direction = launcherData._directionToUse_;

		Vector3[] velocitySplit = S_Interaction_Objects.SplitCoreAndEnvironmentalVelocities(null, direction, launcherData._force_, 0, _maxSpeed, launcherData._useCore);

		//Use the launch data to proejct where player will go.
		return PreviewTrajectory(_ShotCenter.position, velocitySplit[0], velocitySplit[1], launcherData._lockInputTo_);

	}


	//Will take a velocity and important character stats, and create a path the player would follow if they weren't inputting.
	private Vector3[] PreviewTrajectory ( Vector3 position, Vector3 enVelocity, Vector3 coreVelocity, S_GeneralEnums.LockControlDirection whatInput ) {

		//How many fixed update calculations will be performed in space of line length.
		float timeStep = Time.fixedDeltaTime;
		int iterations = _calculations;

		//The two player velocities the spring will set them to.
		_environmentalVelAtStart = enVelocity;
		_coreVelAtEnd = coreVelocity;
		float coreSpeed = coreVelocity.magnitude; //Can afford to use this at this won't be called during gameplay.

		//Ready an array to represent the path
		Vector3[] path = new Vector3[iterations];
		Vector3 pos = position;
		Vector3 vel =  _coreVelAtEnd + _environmentalVelAtStart; //The total velocity each frame
		path[0] = pos;

		for (int i = 1 ; i < iterations ; i++)
		{
			//Affect simulated core velocity using the actual methods.
			_coreVelAtEnd = S_PlayerPhysics.ApplyGravityToIncreaseFallSpeed(_coreVelAtEnd, _fallGravity, _upGravity, _maxFall, vel);
			Vector3 lateralVel = new Vector3(_coreVelAtEnd.x, 0, _coreVelAtEnd.z);

			float decelAmount = _constantDeceleration;
			switch (whatInput)
			{
				//If there is input in a direction, then player will be accelerating, so reflect that.
				case S_GeneralEnums.LockControlDirection.CharacterForwards:
				case S_GeneralEnums.LockControlDirection.Change:
					AccelerateSim(); break;

			}
			coreSpeed -= decelAmount;
			lateralVel = Vector3.MoveTowards(lateralVel, Vector3.zero, decelAmount); //Change magnitude of core velocity.

			_coreVelAtEnd = new Vector3(lateralVel.x, _coreVelAtEnd.y, lateralVel.z); //Ensure vertical velocity is unchanged.

			//Find new point by moving in velocity direction.
			vel = _coreVelAtEnd + _environmentalVelAtStart;
			pos += vel * timeStep;
			path[i] = pos;

			continue;
			void AccelerateSim () {
				float useMod = _accellModInAir;
				if (coreSpeed < 20)
				{
					useMod += 0.5f;
				}
				float acceleration = _acceleration * _AcellBySpeed.Evaluate(coreSpeed / _maxSpeed) * useMod;
				decelAmount -= acceleration;
			}
		}
		return path;
	}

	private void DrawGizmosFromArray ( Vector3[] debugTrajectoryPoints, bool selected ) {

		if (debugTrajectoryPoints == null || debugTrajectoryPoints.Length == 0) { return; }

		if (selected) { Gizmos.color = _selectedOutlineColour; }
		else { Gizmos.color = _normalOutlineColour; }

		//Create a series of line gizmos representing a path along the points.
		for (int i = 1 ; i < debugTrajectoryPoints.Length ; i++)
		{
			Gizmos.DrawLine(debugTrajectoryPoints[i - 1], debugTrajectoryPoints[i]);

			if (i == _lockFrames)
			{
				if (selected) { Gizmos.color = _selectedFillColour; }
				else { Gizmos.color = _selectedOutlineColour; }
				Gizmos.DrawLine(debugTrajectoryPoints[i], debugTrajectoryPoints[i] + Vector3.up * 2);
			}
			else
			{
				Gizmos.DrawLine(debugTrajectoryPoints[i], debugTrajectoryPoints[i] + Vector3.up);
			}
		}
	}
#endif
}
