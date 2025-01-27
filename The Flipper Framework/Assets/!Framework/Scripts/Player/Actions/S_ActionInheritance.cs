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

	[HideInInspector] public int            _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 


	public bool AttemptAction () {
		return false;
	}

	public void StartAction ( bool overwrite = false ) {

	}

	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			if(this is ISubAction) { return; } //_positionInActionList is only used for MainActions, so only continue if not a sub action.

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].Action == this as IMainAction)
				{
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
	}
}

/// <summary>
/// Interfaces
/// </summary>

public interface IAction
{
	bool AttemptAction ();

	void StartAction ( bool overwrite = false );

	//void ReactivateAction ();

	//void DeactivateAction ();
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
