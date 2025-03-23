using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Base class for actions to inherit 
public class S_Action_Base : MonoBehaviour, IAction
{
	[HideInInspector] public S_CharacterTools      _Tools;
	[HideInInspector] public S_PlayerPhysics       _PlayerPhys;
	[HideInInspector] public S_PlayerVelocity      _PlayerVel;
	[HideInInspector] public S_PlayerInput         _Input;
	[HideInInspector] public S_ActionManager       _Actions;
	[HideInInspector] public S_Handler_Camera      _CamHandler;
	[HideInInspector] public S_Control_SoundsPlayer _Sounds;
	[HideInInspector] public S_PlayerMovement      _PlayerMovement;

	[HideInInspector] public Animator              _CharacterAnimator;
	[HideInInspector] public GameObject            _JumpBall;
	[HideInInspector] public Animator              _BallAnimator;
	[HideInInspector] public Transform             _MainSkin;
	[HideInInspector] public CapsuleCollider       _CharacterCapsule;

	[HideInInspector] public int            _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 
	[HideInInspector] public bool           _isActionCurrentlyValid = true;       //Controlled by Activate And Deactivate action. Can't perform actions if false.[Hide

	[HideInInspector] public int        _framesWithoutLocalCheckActionCalled; //Increases every frame, but set to zero when AttemptAction is called, if it reaches 3, then sets the below to false.
	[HideInInspector] public bool       _inAStateConnectedToThis;        //Used by children to check when should end a state, of if possible to enter.
	[HideInInspector] public bool           _canEnterStateFromSelf; //Set by inherited classes (not per instance), to allow an action to call itself.

	public bool AttemptAction () {
		if(!_isActionCurrentlyValid) { return false; }

		return true;
	}

	public void StartAction ( bool overwrite = false ) {

	}

	public void FixedUpdate () {
		if (_Actions == null) { return; }
		string debugThisAction = this.ToString();

		_Actions.CheckConnectedActions(_positionInActionList);
	}

	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			if (this is ISubAction) { return; } //_positionInActionList is only used for MainActions, so only continue if not a sub action.

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].Action == this as IMainAction)
				{
					_isActionCurrentlyValid = true;
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	public virtual void AssignStats () {

	}

	public virtual void AssignTools () {
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerMovement = _Tools.GetComponent<S_PlayerMovement>();

		_Actions = _Tools._ActionManager;
		_CamHandler = _Tools.CamHandler;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_BallAnimator = _Tools.BallAnimator;
		_MainSkin = _Tools.MainSkin;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;
		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
	}

	public void ReactivateAction () {
		_isActionCurrentlyValid = true;
	}

	public void DeactivateAction () {
		if(this is S_Action00_Default) { return; } //Default action cannot be deactivated.

		_isActionCurrentlyValid = false;

		//If this is a main action and currently in use when deactivated, immediately return to Default Action.
		if(enabled && this is IMainAction) { _Actions._ActionDefault.StartAction(); }
	}

	//Responsible for taking in inputs the player performs to switch or activate other actions, or other effects.
	public virtual void HandleInputs () {
		//Moving camera behind
		if (!_Actions._isPaused) _CamHandler.AttemptCameraReset();

		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);
	}

	public virtual void ActionEveryFixedUpdate () {
		string debugThisAction = this.ToString();
		_inAStateConnectedToThis = _framesWithoutLocalCheckActionCalled < 3;
		if(this is IMainAction && enabled) { _inAStateConnectedToThis = _canEnterStateFromSelf; }
		_framesWithoutLocalCheckActionCalled++;
	}

	public void CheckAction () {
		_framesWithoutLocalCheckActionCalled = 0;
	}

}

/// <summary>
/// Interfaces
/// </summary>

public interface IAction
{
	bool AttemptAction ();

	void CheckAction ();

	void ActionEveryFixedUpdate ();

	void StartAction ( bool overwrite = false );

	void ReactivateAction ();

	void DeactivateAction ();
}

public interface IMainAction : IAction
{
	void HandleInputs ();

	void StopAction ( bool isFirstTime = false );
}

public interface ISubAction : IAction
{

}


public interface IControlAction
{

}
