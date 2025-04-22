using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using SplineMesh;
using System.Collections.Generic;
//using Luminosity.IO;


[RequireComponent(typeof(S_RailFollow_Base))]
//[RequireComponent(typeof(Spline))]
[RequireComponent(typeof(Rigidbody))]
public class S_AI_RailEnemy : MonoBehaviour, ITriggerable
{

	[AsButton("Set To Spline", "SetToSpline", null)]
	[SerializeField] bool SetToSplineButton;

	[Header("Tools")]
	[HideInInspector,SerializeField]
	private S_RailFollow_Base _RF;
	private Rigidbody _RB;
	[SerializeField, ColourIfNull(0.8f,0.65f,0.65f,1)] private Animator _Animator;

	public GameObject[] _Models;
	public Vector3[] _modelStartRotations;
	public float _disanceBetweenModels = 30;

	[Header("Start Rails")]
	public Spline _StartSpline;
	public S_AddOnRail _StartingConnectedRails;
	public Vector3 _startOffset;

	[Header("Type")]
	[DrawHorizontalWithOthers(new string[]{"_armouredTrain"})]
	public bool _rhino;
	[HideInInspector]
	public bool _armouredTrain;

	[Header("Control")]
	[SerializeField] float StartSpeed = 60f;
	[SerializeField] float _railUpOffset;
	[SerializeField] bool _followPlayer;
	[SerializeField] bool _isBackwards;

	[Header("Stats")]
	[SerializeField] AnimationCurve _timeToFullSpeed_ = new AnimationCurve(new Keyframe[] { new Keyframe (0,0), new Keyframe(1,1) });
	[SerializeField] AnimationCurve _FollowByDistance_;
	[SerializeField] AnimationCurve _FollowBySpeedDif_;
	[SerializeField] float _followLerpSpeed_ = 0.3f;
	[SerializeField] float _distanceAheadOfPlayerToAimFor;
	//[SerializeField] float _slopePower_ = 2.5f;

	bool _isActive = false;
	[HideInInspector] public S_Action05_Rail _PlayerRailAction;
	private S_RailFollow_Base _PlayerRF;
	[HideInInspector] public S_ActionManager _PlayerActions;

	private S_PlayerVelocity _PlayerVel;

	float _playerDistance;
	float _playerSpeed;
	List<float> _listOfPlayerSpeeds = new List<float> {20,20,20,20,20,20,20};
	float _timeGrinding;

	Vector3 _startPosition;
	private bool _isFirstSet = true;

	#region inherited

	private void Start () {
		_RF = GetComponent<S_RailFollow_Base>();
		_RB = GetComponent<Rigidbody>();

		ResetRigidBody();

		if (!_StartSpline) { return; }

		SetSplineDetails();

		PlaceOnSplineToStart();
		_startPosition = transform.position;
		_RF._setOffSet = -_startOffset;
	}

	private void Update () {
		if (!_isActive) { return; }

		_RF.CustomUpdate();
		_RF.PlaceOnRail(SetRotation, SetPosition);
	}

	private void FixedUpdate () {
		if (!_isActive) { return; }

		_RF.CustomFixedUpdate();
		_RF.PlaceOnRail(SetRotation, SetPosition);
		MoveOnRail();
		HandleGrindSpeed();

		if (_RF._isRailLost)
			LoseRail();
	}

	private void OnDestroy () {
		//Debug.Log("Killed itself lmao");
	}

	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		SetIsActive(true);
		_RF.StartOnRail();

		if (!_isActive) { return; }

		_RF._grindingSpeed = StartSpeed * _timeToFullSpeed_.Evaluate(0);

		_PlayerVel = Player._PlayerVelocity;
		_PlayerActions = Player.GetComponent<S_CharacterTools>()._ActionManager;
		_PlayerRailAction = _PlayerActions.GetComponentInChildren<S_Action05_Rail>();
		if (_PlayerRailAction) _PlayerRF = _PlayerRailAction._RF;

		S_Manager_LevelProgress.OnReset += EventReturnOnDeath;
	}

	public void TriggerObjectOff ( S_PlayerPhysics Player = null ) {
		SetIsActive(false);
		_RF._grindingSpeed = 0;
	}
	#endregion

	private void SetIsActive ( bool set ) {
		_isActive = set;
		_timeGrinding = 0;

		if (set) { SettingAnimationTrigger("Start"); }

		if (!_RF || !_RF._PathSpline) { _isActive = false; }
	}

	/// 
	///	PLACING
	/// 
	#region Placing

	public void SetPosition ( Vector3 position ) {
		transform.position = position;
	}

	public void SetRotation ( S_Interaction_Pathers.PathTypes pathType ) {
		AlignCars();
	}

	void AlignCars () {
		if (_Models.Length == 0)
		{
			transform.rotation = Quaternion.LookRotation(_RF._sampleForwards, _RF._sampleUpwards);
			return;
		}

		float tempPointOnSpline = _RF._pointOnSpline;
		Spline thisSpline = _RF._PathSpline;

		for (int i = 0 ; i < _Models.Length ; i++)
		{
			GameObject ModelObject = _Models[i];
			//Crossing models over the loop
			if (_RF._PathSpline.IsLoop && (tempPointOnSpline < 0 || tempPointOnSpline > thisSpline.Length))
			{
				tempPointOnSpline += thisSpline.Length * -_RF._movingDirection;

			}
			//If this car is on a previous 
			else if (_RF._ConnectedRails != null)
			{
				if (tempPointOnSpline < 0 && !_isBackwards && _RF._ConnectedRails.PrevRail != null && _RF._ConnectedRails.PrevRail.isActiveAndEnabled)
				{
					thisSpline = _RF._ConnectedRails.PrevRail.GetComponentInParent<Spline>();
					tempPointOnSpline += thisSpline.Length;
				}
				else if (tempPointOnSpline > thisSpline.Length && _isBackwards && _RF._ConnectedRails.NextRail != null && _RF._ConnectedRails.NextRail.isActiveAndEnabled)
				{
					tempPointOnSpline -= thisSpline.Length;
					thisSpline = _RF._ConnectedRails.PrevRail.GetComponentInParent<Spline>();
				}
			}

			CurveSample TempSample = thisSpline.GetSampleAtDistance(Mathf.Clamp(tempPointOnSpline, 0, thisSpline.Length - 0.5f));
			SetTransformDirectly(TempSample, ModelObject, _modelStartRotations[i]);

			tempPointOnSpline -= _disanceBetweenModels * _RF._movingDirection;
		}

	}

	void SetTransformDirectly ( CurveSample ThisSample, GameObject ThisObject, Vector3 localEuler ) {

		Spline.SampleTransforms ThisSampleTransform = Spline.GetSampleTransformInfo(_RF._RailTransform, ThisSample);

		ThisObject.transform.position = ThisSampleTransform.location + (ThisSampleTransform.upwards * _railUpOffset);
		Quaternion newRotation = Quaternion.LookRotation(ThisSampleTransform.forwards, ThisSampleTransform.upwards);
		newRotation *= Quaternion.Euler(localEuler);
		ThisObject.transform.rotation = newRotation;

	}
	#endregion

	/// 
	///	PHYSICS
	/// 
	#region Physics
	public void MoveOnRail () {
		_RB.velocity = _RF._sampleForwards * _RF._grindingSpeed;
	}

	private void HandleGrindSpeed () {
		_timeGrinding += Time.deltaTime;

		//If the curve has no nodes.
		if (_timeToFullSpeed_.length == 0) { return; }

		//Getting to full speed from start. Compare time grinding to last node.
		if (_timeGrinding < _timeToFullSpeed_[_timeToFullSpeed_.length - 1].time)
		{
			_RF._grindingSpeed = StartSpeed * _timeToFullSpeed_.Evaluate(_timeGrinding);
			_timeGrinding = Mathf.Min(_timeToFullSpeed_[_timeToFullSpeed_.length - 1].time, _timeGrinding);
		}
		//Normal speed control
		else
		{
			TrackPlayer();
			SlopePhysics();
		}
	}

	void TrackPlayer () {
		if (!_followPlayer) return;

		_listOfPlayerSpeeds.Insert(0, _PlayerVel._horizontalSpeedMagnitude);
		_listOfPlayerSpeeds.RemoveAt(_listOfPlayerSpeeds.Count - 1);
		_playerSpeed = _listOfPlayerSpeeds[_listOfPlayerSpeeds.Count-1]; //The player speed to lerp to is delayed by x frames.

		float goalSpeed = _playerSpeed;

		float lerpSpeed = _followLerpSpeed_;

		//If player is also grinding.
		if (_PlayerActions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Rail && (OnSameRailOrConnected()))
		{
			float modi = _FollowByDistance_.Evaluate(_playerDistance);
			lerpSpeed *= modi;
			goalSpeed = _playerDistance < 0 ? goalSpeed / modi : goalSpeed * modi;
		}
		else
		{
			lerpSpeed *= _FollowBySpeedDif_.Evaluate(Mathf.Abs(_PlayerRF._grindingSpeed - _playerSpeed));
		}

		Debug.Log(lerpSpeed + " To " + goalSpeed);

		_RF._grindingSpeed = Mathf.Lerp(_RF._grindingSpeed, goalSpeed, lerpSpeed);

		return;

		bool OnSameRailOrConnected () {
			bool onSameRailOrConnected = _PlayerRF._PathSpline == _RF._PathSpline;
			float thisPointOnSplines = _RF._pointOnSpline;
			float playerPointOnSplines = _PlayerRF._pointOnSpline;

			//Player on enemy's next rail
			if (!onSameRailOrConnected && _PlayerRF._ConnectedRails.PrevRail)
			{
				onSameRailOrConnected = _PlayerRF._ConnectedRails.PrevRail._Spline == _RF._PathSpline;
				//Player spline position has enemy's spline added on, to treat them as one continue length.
				if (onSameRailOrConnected) { playerPointOnSplines += _RF._PathSpline.Length; }
			}
			//Player on enemy's previous rail
			if (!onSameRailOrConnected && _PlayerRF._ConnectedRails.NextRail)
			{
				onSameRailOrConnected = _PlayerRF._ConnectedRails.NextRail._Spline == _RF._PathSpline;
				//Player spline position treated as how far from the enemy's rail (E.G. -200)
				if (onSameRailOrConnected) playerPointOnSplines = playerPointOnSplines = _PlayerRF._pointOnSpline - _PlayerRF._PathSpline.Length;
			}
			//Share next rail
			if (!onSameRailOrConnected && _PlayerRF._ConnectedRails.NextRail && _RF._ConnectedRails.NextRail)
			{
				onSameRailOrConnected = _PlayerRF._ConnectedRails.NextRail._Spline == _RF._ConnectedRails.NextRail._Spline;
				//Both spline positions treated as how far from the upcoming rail (E.G. -100 & -150)
				if (onSameRailOrConnected) { thisPointOnSplines = _RF._pointOnSpline - _RF._PathSpline.Length; playerPointOnSplines = _PlayerRF._pointOnSpline - _PlayerRF._PathSpline.Length; }
			}
			//Share Prev Rail
			if (!onSameRailOrConnected && _PlayerRF._ConnectedRails.PrevRail && _RF._ConnectedRails.PrevRail)
			{
				onSameRailOrConnected = _PlayerRF._ConnectedRails.PrevRail._Spline == _RF._ConnectedRails.PrevRail._Spline;
			}

			if (onSameRailOrConnected)
			{
				//How far ahead or behind the player is considering the enemy's direction.
				_playerDistance = (thisPointOnSplines - (playerPointOnSplines + _PlayerRF._movingDirection * _distanceAheadOfPlayerToAimFor)) * -_RF._movingDirection;
			}
			return onSameRailOrConnected;
		}
	}


	void SlopePhysics () {
		if (Mathf.Abs(_RF._sampleUpwards.y) < 0.1)
		{

		}
	}
	#endregion

	/// 
	///	SETTING VALUES	
	/// 
	#region Setting Values

	private void SettingAnimationTrigger ( string trigger ) {
		if (!_rhino || !_Animator) { return; }

		_Animator.SetTrigger(trigger);

		if (trigger == "Start") { _Animator.SetBool("IsActive", true); }
		else if (trigger == "Stop") { _Animator.SetBool("IsActive", false); }
	}

	private void SetSplineDetails () {
		if (!_StartSpline) { return; }
		_RF._ConnectedRails = _StartingConnectedRails;
		_RF._PathSpline = _StartSpline;
		_RF._RailTransform = _StartSpline.transform;

		_RF._grindingSpeed = 0;
		_RF._isGoingBackwards = _isBackwards;
		_RF._upOffsetRail_ = _railUpOffset;
	}

	private void PlaceOnSplineToStart () {
		if (!_RF || !_StartSpline) return;


		Vector2 rangeAndDistanceSquared = S_RailFollow_Base.GetClosestPointOfSpline(transform.position, _StartSpline, Vector3.zero);
		_RF._pointOnSpline = rangeAndDistanceSquared.x;

		if (_isFirstSet)
		{
			_modelStartRotations = new Vector3[_Models.Length];
			for (int i = 0 ; i < _Models.Length ; i++)
			{
				Vector3 localRotation = _Models[i].transform.localEulerAngles;
				_modelStartRotations[i] = localRotation;
			}
			if (Application.isPlaying) { _isFirstSet = false; }
		}

		float maxSpace = _Models.Length * _disanceBetweenModels;
		_RF._pointOnSpline = Mathf.Clamp(_RF._pointOnSpline, maxSpace, _StartSpline.Length - maxSpace);
		_RF.GetNewSampleOnRail(0);

		_RF._upOffsetRail_ = _railUpOffset;

		Vector3 thisOffset = _RF._sampleRotation * _startOffset;

		transform.position = _RF._sampleLocation + thisOffset;
		transform.rotation = Quaternion.LookRotation(_RF._sampleForwards, _RF._sampleUpwards);
		AlignCars();
	}
	#endregion

	/// 
	///	ENDING RAIL
	/// 
	#region endingRail
	public void LoseRail () {
		if (_rhino)
		{
			_RB.freezeRotation = false;
			_RB.useGravity = true;
		}
		else if (_armouredTrain)
		{
			_RF._grindingSpeed = 0;
			_RB.velocity = Vector3.zero;
		}
		_RF._grindingSpeed = 0;
		_isActive = false;
	}

	private void ResetRigidBody () {
		_RB.velocity = Vector3.zero;
		_RB.useGravity = false;
		_RB.freezeRotation = true;
	}

	void EventReturnOnDeath ( object sender, EventArgs e ) {
		if (!gameObject) { return; }
		gameObject.SetActive(true);

		TriggerObjectOff();

		transform.position = _startPosition;
		SetSplineDetails();
		PlaceOnSplineToStart();
		ResetRigidBody();

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}

	public void SetToSpline () {
		_RF = GetComponent<S_RailFollow_Base>();
		SetSplineDetails();
		PlaceOnSplineToStart();
	}

	#endregion
}

