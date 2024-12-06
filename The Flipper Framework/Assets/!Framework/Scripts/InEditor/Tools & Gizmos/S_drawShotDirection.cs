using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
public class S_drawShotDirection : MonoBehaviour
{
	[Header("Gizmo")]
	[Tooltip("If true, creates a series of gizmos that show the trajectory of the player after being launched by this. The red lines are the path and positions every frame. The blue line represents when the control lock ends.")]
	public bool	_debugForce;
	[Range(0f, 150f), Tooltip("This is how many phyiscs checks this gizmo should make. This means how many FixedUpdates would be called for the player when calculating their movement after the bounce.")]
	public int	_calculations = 1;

	[Header("Launcher")]
	[Tooltip("If you put a spring script here, the gizmo will calculate based on values from that script instance.")]
	public S_Data_Spring	_SpringScript;
	[Tooltip("If you put a speed pad script here, the gizmo will calculate based on values from that script instance.")]
	public S_Data_SpeedPad	_DashRingScript;


	[Header("Physics")]
	[Tooltip("Set by the script instance above.")]
	public float	_force;
	[Tooltip("Where to originate the gizmo from. Can just be the transform of this same object.")]
	public Transform	_ShotCenter;
	private Vector3	_overwriteGravity = Vector3.zero;

	[Tooltip("Put is some character stats and the gizmo will show the trajectory that character would take. Including gravity and deceleration differences.")]
	public S_O_CharacterStats _CharacterStatsToFollow;

	private Vector3	_fallGravity = new Vector3(0, -1.5f, 0);
	private Vector3	_upGravity = new Vector3(0, -1.7f, 0);
	private float       _maxFall;
	private float	_constantDeceleration;
	private float	_acceleration;
	private float       _accellModInAir;
	private AnimationCurve _AcellBySpeed;
	private float        _maxSpeed;

	private int         _lockFrames;

	public Vector3 _simCoreVel;
	public Vector3 _simEnVel;


	//Called whenever object is selected when gizmos are enabled.
	private void OnDrawGizmosSelected () {
		if (_debugForce && _calculations > 0) //Will only show line if there's a line to create.
		{
			//Gets values for simulation from stats object.
			_constantDeceleration =	_CharacterStatsToFollow.DecelerationStats.airConstantDecel;
			_fallGravity =		_CharacterStatsToFollow.WhenInAir.fallGravity;
			_upGravity =		_CharacterStatsToFollow.WhenInAir.upGravity;
			_maxFall =		_CharacterStatsToFollow.WhenInAir.startMaxFallingSpeed;
			_acceleration =		_CharacterStatsToFollow.AccelerationStats.runAcceleration;
			_accellModInAir =		_CharacterStatsToFollow.WhenInAir.controlAmmount.y;
			_AcellBySpeed =		_CharacterStatsToFollow.AccelerationStats.AccelBySpeed;
			_maxSpeed =		_CharacterStatsToFollow.SpeedStats.maxSpeed;

			Vector3[] DebugTrajectoryPoints; //This array will make points along a line following the path the player should take.
			if (_SpringScript)
			{
				//If spring, get gravity, force and duration of lock.
				_force = _SpringScript._springForce_;

				_overwriteGravity = _SpringScript._overwriteGravity_;
				if(_overwriteGravity != Vector3.zero)
				{
					_fallGravity = _overwriteGravity;
					_upGravity = _overwriteGravity;
				}
				_lockFrames = _SpringScript._lockForFrames_; 

				//Use the launch data to proejct where player will go.
				DebugTrajectoryPoints = PreviewTrajectory(_ShotCenter.position, _SpringScript._BounceTransform.up * _force, _SpringScript._BounceTransform.up * 2, _SpringScript._LockInputTo_);
			}
			else
			{
				_force = _DashRingScript._speedToSet_;
				_overwriteGravity = _DashRingScript._overwriteGravity_;
				if (_overwriteGravity != Vector3.zero)
				{
					_fallGravity = _overwriteGravity;
					_upGravity = _overwriteGravity;
				}
				_lockFrames = _DashRingScript._lockControlFrames_;

				//Use the launch data to proejct where player will go.
				DebugTrajectoryPoints = PreviewTrajectory(_ShotCenter.position, transform.forward * _force, transform.forward * 2, _DashRingScript._lockInputTo_);
			}

			//Create a series of line gizmos representing a path along the points.
			for (int i = 1 ; i < DebugTrajectoryPoints.Length ; i++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(DebugTrajectoryPoints[i - 1], DebugTrajectoryPoints[i]);

				if(i == _lockFrames)
				{
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(DebugTrajectoryPoints[i], DebugTrajectoryPoints[i] + Vector3.up * 2);
				}
				else
				{
					Gizmos.DrawLine(DebugTrajectoryPoints[i], DebugTrajectoryPoints[i] + Vector3.up);
				}
			}		
		}
	}

	//Will take a velocity and important character stats, and create a path the player would follow if they weren't inputting.
	private Vector3[] PreviewTrajectory ( Vector3 position, Vector3 enVelocity, Vector3 coreVelocity, S_GeneralEnums.LockControlDirection whatInput) {

		//How many fixed update calculations will be performed in space of line length.
		float timeStep = Time.fixedDeltaTime;
		int iterations = _calculations;

		//The two player velocities the spring will set them to.
		_simEnVel = enVelocity;
		_simCoreVel = coreVelocity;
		float coreSpeed = coreVelocity.magnitude; //Can afford to use this at this won't be called during gameplay.

		//Ready an array to represent the path
		Vector3[] path = new Vector3[iterations];
		Vector3 pos = position;
		Vector3 vel =  _simCoreVel + _simEnVel; //The total velocity each frame
		path[0] = pos;

		for (int i = 1 ; i < iterations ; i++)
		{
			//Affect simulated core velocity using the actual methods.
			_simCoreVel = S_PlayerPhysics.ApplyGravityToIncreaseFallSpeed(_simCoreVel, _fallGravity, _upGravity, _maxFall, vel);
			Vector3 lateralVel = new Vector3(_simCoreVel.x, 0, _simCoreVel.z);

			float decelAmount = _constantDeceleration;
			switch (whatInput)
			{
				//If there is input in a direction, then player will be accelerating, so reflect that.
				case S_GeneralEnums.LockControlDirection.CharacterForwards: 
					float useMod = _accellModInAir;
					if(coreSpeed < 20)
					{
						useMod += 0.5f;
					}
					decelAmount -= _acceleration * _AcellBySpeed.Evaluate(coreSpeed / _maxSpeed) * useMod; break;

			}
			coreSpeed -= decelAmount;
			lateralVel = Vector3.MoveTowards(lateralVel, Vector3.zero, decelAmount); //Change magnitude of core velocity.

			_simCoreVel = new Vector3(lateralVel.x, _simCoreVel.y, lateralVel.z); //Ensure vertical velocity is unchanged.

			//Find new point by moving in velocity direction.
			vel = _simCoreVel + _simEnVel;
			pos += vel * timeStep;
			path[i] = pos;
		}
		return path;
	}
}
#endif
