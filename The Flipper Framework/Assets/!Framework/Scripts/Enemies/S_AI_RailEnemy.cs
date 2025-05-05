using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using SplineMesh;
using System.Collections.Generic;
using Unity.VisualScripting;
//using Luminosity.IO;

[Serializable]
public class S_RailEnemyData
{
	[Header("Start Rails")]
	public Spline _StartSpline_;
	public S_AddOnRail _StartingConnectedRails_;
	public Vector3 _startOffset_;
	public float _startDistance_ = -1;

	[Header("Type")]
	[DrawHorizontalWithOthers(new string[]{"_armouredTrain_"})]
	public bool _rhino_;
	[HideInInspector]
	public bool _armouredTrain_;

	[Header("Control")]
	public float _startSpeed_ = 60f;
	public float _minimumSpeed_ = 60f;
	public float _railUpOffset_;
	public bool _followPlayer_;
	public bool _isBackwards_;

	[Header("Stats")]
	public AnimationCurve _CurveToFullSpeed_ = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
	public AnimationCurve _LerpFollowByDistance_;
	public AnimationCurve _FollowBySpeedDifference_;
	public float _followLerpSpeed_ = 0.3f;
	public float _distanceAheadOfPlayerToAimFor_;
}


[RequireComponent(typeof(S_RailFollow_Base))]
//[RequireComponent(typeof(Spline))]
[RequireComponent(typeof(Rigidbody))]
public class S_AI_RailEnemy : MonoBehaviour, ITriggerable
{

	[AsButton("Set To Spline", "SetToSpline", null)]
	[SerializeField] bool SetToSplineButton;

	[Header("Tools")]
	[HideInInspector,SerializeField]
	public S_RailFollow_Base _RF;
	[HideInInspector] public Rigidbody _RB;
	[ColourIfNull(0.8f,0.65f,0.65f,1)] public Animator _Animator;

	//See class above. Stored in a seperate class for organised editor and S_AI_RhinoMaster
	public S_RailEnemyData _Data;

	[Space]
	//If a train with multiple cars, must be placed carefully
	public GameObject[] _Models;
	public Vector3[] _modelStartRotations;
	public float _disanceBetweenModels = 30;

	bool _isActive = false;

	//Player Data
	[HideInInspector] public S_Action05_Rail _PlayerRailAction;
	private S_RailFollow_Base _PlayerRF;
	[HideInInspector] public S_ActionManager _PlayerActions;
	private S_PlayerVelocity _PlayerVel;

	//Tracking the player and reacting
	float _playerDistanceIncludingOffset;
	float _playerDistanceWithoutOffset;
	float _playerSpeed;
	List<float> _listOfPlayerSpeeds = new List<float> {20,20,20,20,20,20,20,20,20};
	float _timeGrinding;
	private bool _hasReachedGoalInFrontOfPlayer; //Set to true when hit goal distance ahead of player, and false when behind the player themselves. This will increase speed when catching up to position ahead, but allow player to catch up.

	public event System.Action<GameObject> OnGetInFrontOfPlayer;
	public event System.Action<GameObject> OnFallBehindPlayer;

	//At start
	Vector3 _startPosition;
	private bool _isFirstSet = true;

	#region inherited

	private void Start () {
		_RF = GetComponent<S_RailFollow_Base>();
		_RB = GetComponent<Rigidbody>();

		if (OnGetInFrontOfPlayer != null)
			Debug.Log(OnGetInFrontOfPlayer.GetInvocationList().Length);

		ResetRigidBody();

		if (!_Data._StartSpline_) { return; }

		ReSetSplineDetails();

		PlaceOnSplineBeforeGame();
		_startPosition = transform.position;
		_RF._setOffSet = -_Data._startOffset_;

		SetAnimatorBool("CurrentlyOnRail", true);
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
		Debug.Log("Killed itself lmao");
	}

	public void TriggerObjectOnce ( S_PlayerPhysics Player = null ) {
		SetIsActive(true);
		_RF.StartOnRail();

		if (!_isActive) { return; }

		_RF._grindingSpeed = _Data._startSpeed_ * _Data._CurveToFullSpeed_.Evaluate(0);

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

		if (set) { SetAnimatorTrigger("Start"); }

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
				if (tempPointOnSpline < 0 && !_Data._isBackwards_ && _RF._ConnectedRails.PrevRail != null && _RF._ConnectedRails.PrevRail.isActiveAndEnabled)
				{
					thisSpline = _RF._ConnectedRails.PrevRail.GetComponentInParent<Spline>();
					tempPointOnSpline += thisSpline.Length;
				}
				else if (tempPointOnSpline > thisSpline.Length && _Data._isBackwards_ && _RF._ConnectedRails.NextRail != null && _RF._ConnectedRails.NextRail.isActiveAndEnabled)
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

		ThisObject.transform.position = ThisSampleTransform.location + (ThisSampleTransform.upwards * _Data._railUpOffset_);
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
		if (_Data._CurveToFullSpeed_.length == 0) { return; }

		//Getting to full speed from start. Compare time grinding to last node.
		if (_timeGrinding < _Data._CurveToFullSpeed_[_Data._CurveToFullSpeed_.length - 1].time)
		{
			_RF._grindingSpeed = _Data._startSpeed_ * _Data._CurveToFullSpeed_.Evaluate(_timeGrinding);
			_timeGrinding = Mathf.Min(_Data._CurveToFullSpeed_[_Data._CurveToFullSpeed_.length - 1].time, _timeGrinding);
		}
		//Normal speed control
		else
		{
			TrackPlayer();
			SlopePhysics();

			_RF._grindingSpeed = Mathf.Max(_RF._grindingSpeed, _Data._minimumSpeed_);
		}
	}

	//Adusts speed to a goal speed based on player's current state and position.
	void TrackPlayer () {
		if (!_Data._followPlayer_) return;

		_listOfPlayerSpeeds.Insert(0, _PlayerVel._horizontalSpeedMagnitude);
		_listOfPlayerSpeeds.RemoveAt(_listOfPlayerSpeeds.Count - 1);
		_playerSpeed = _listOfPlayerSpeeds[_listOfPlayerSpeeds.Count - 1]; //The player speed to lerp to is delayed by x frames.

		float goalSpeed = _playerSpeed;
		float lerpSpeed = _Data._followLerpSpeed_;

		//If player is also grinding.
		if (_PlayerActions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Rail && (OnSameRailOrConnected()))
		{
			float modi = _hasReachedGoalInFrontOfPlayer ? _Data._LerpFollowByDistance_.Evaluate(_playerDistanceWithoutOffset) : _Data._LerpFollowByDistance_.Evaluate(_playerDistanceIncludingOffset);
			lerpSpeed *= modi;
			goalSpeed = _playerDistanceIncludingOffset < 0 ? goalSpeed / modi : goalSpeed * modi; // If enemy is ahead of goal position, decrease goal speed. If behind, increase goal speed.

			//If ahead of player not including goal position, allow player a chance to catch up.
			if (_hasReachedGoalInFrontOfPlayer && goalSpeed > 100)
			{
				goalSpeed -= 10f;
				if (goalSpeed > _RF._grindingSpeed)
				{ lerpSpeed *= 0.65f; }
			}
		}
		else if (_PlayerActions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Homing
			&& S_S_MoreMaths.GetDistanceSqrOfVectors(_PlayerActions._currentTargetPosition, transform.position) < 10 * 10) //If player is homing in on this, slow down to allow the hit to be made.
		{
			goalSpeed = _RF._grindingSpeed * 0.98f;
			lerpSpeed /= 2;
		}
		else
		{
			if (Vector3.Angle(_PlayerVel._worldVelocity.normalized, (transform.position - _PlayerVel.transform.position).normalized) < 90)
				SetHasReachedGoalInFrontOfPlayer(true); //If player's direction is taking them towards the rhinos, then the rhinos are in front.
			else SetHasReachedGoalInFrontOfPlayer(false);

			lerpSpeed *= _Data._FollowBySpeedDifference_.Evaluate(Mathf.Abs(_RF._grindingSpeed - _playerSpeed));
		}

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
				_playerDistanceIncludingOffset = (thisPointOnSplines - (playerPointOnSplines + _PlayerRF._movingDirection * _Data._distanceAheadOfPlayerToAimFor_)) * -_RF._movingDirection;
				_playerDistanceWithoutOffset = (thisPointOnSplines - playerPointOnSplines) * -_RF._movingDirection;

				if (_playerDistanceIncludingOffset <= 0) SetHasReachedGoalInFrontOfPlayer(true); //Reached point ahead of player to be.
				else if (_playerDistanceWithoutOffset > 0) SetHasReachedGoalInFrontOfPlayer(false); //Has fallen behiond the player properly.
			}
			return onSameRailOrConnected;
		}
	}

	private void SetHasReachedGoalInFrontOfPlayer ( bool set ) {
		if (_hasReachedGoalInFrontOfPlayer != set)
		{
			_hasReachedGoalInFrontOfPlayer = set;
			if (set && OnGetInFrontOfPlayer != null) { OnGetInFrontOfPlayer.Invoke(gameObject); }
			else if (OnFallBehindPlayer != null) { OnFallBehindPlayer.Invoke(gameObject); }
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

	public void SetAnimatorTrigger ( string trigger ) {
		if (!_Data._rhino_ || !_Animator) { return; }

		_Animator.SetTrigger(trigger);

		if (trigger == "Start") { _Animator.SetBool("IsActive", true); }
		else if (trigger == "Stop") { _Animator.SetBool("IsActive", false); }
	}

	public void SetAnimatorBool ( string boolean, bool set ) {
		if (!_Data._rhino_ || !_Animator) { return; }

		_Animator.SetBool(boolean, set);
	}

	private void ReSetSplineDetails () {
		if (!_Data._StartSpline_) { return; }
		SetRail(_Data._StartSpline_, _Data._StartingConnectedRails_);

		_RF._grindingSpeed = 0;
		_RF._isGoingBackwards = _Data._isBackwards_;
		_RF._upOffsetRail_ = _Data._railUpOffset_;
	}

	public void SetRail (Spline Spline, S_AddOnRail AddOns) {
		_RF._ConnectedRails = AddOns;
		_RF._PathSpline = Spline;
		_RF._RailTransform = Spline.transform;
	}

	private void PlaceOnSplineBeforeGame () {
		if (!_RF || !_Data._StartSpline_ || _Data._StartSpline_.Length < _Models.Length * _disanceBetweenModels * 3) return;

		//If set to a specific start distance, get placed there.
		if (_Data._startDistance_ > 0 && _Data._startDistance_ < _Data._StartSpline_.Length)
			_RF._pointOnSpline = _Data._startDistance_;
		//If not, find the current closest space.
		else
		{
			Vector2 rangeAndDistanceSquared = S_RailFollow_Base.GetClosestPointOfSpline(transform.position, _Data._StartSpline_, Vector3.zero);
			_RF._pointOnSpline = rangeAndDistanceSquared.x;
		}

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

		float maxSpace = _Models.Length * _disanceBetweenModels; //Ensures all cars can fit on spline.
		_RF._pointOnSpline = Mathf.Clamp(_RF._pointOnSpline, maxSpace, _Data._StartSpline_.Length - maxSpace);
		_RF.GetNewSampleOnRail(0); //To ensure struct in _RF is up to date.

		_RF._upOffsetRail_ = _Data._railUpOffset_;

		Vector3 thisOffset = _RF._sampleRotation * _Data._startOffset_; //Add offset to allow multiple rails from one spline.

		//Move to spline
		transform.position = _RF._sampleLocation + thisOffset;
		transform.rotation = Quaternion.LookRotation(_RF._sampleForwards, _RF._sampleUpwards);
		AlignCars(); //If armoured train, place cars along behind.
	}
	#endregion

	/// 
	///	ENDING RAIL
	/// 
	#region endingRail
	public void LoseRail () {
		if (_Data._rhino_)
		{
			_RB.freezeRotation = false;
			_RB.useGravity = true;
		}
		else if (_Data._armouredTrain_)
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
		ReSetSplineDetails();
		PlaceOnSplineBeforeGame();
		ResetRigidBody();

		//Reset tracking values
		_hasReachedGoalInFrontOfPlayer = false;
		_playerDistanceIncludingOffset = 0;
		_playerDistanceWithoutOffset = 0;
		_playerSpeed = 0;
		_listOfPlayerSpeeds = new List<float> { 20, 20, 20, 20, 20, 20, 20, 20, 20 };
		_timeGrinding = 0;

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}

	public void SetToSpline () {
		_RF = GetComponent<S_RailFollow_Base>();
		ReSetSplineDetails();
		PlaceOnSplineBeforeGame();
	}

	#endregion
}

