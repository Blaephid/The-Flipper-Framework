using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Player_Base : MonoBehaviour
{
	public S_CharacterTools      _Tools;
	[NonSerialized] public S_PlayerPhysics       _PlayerPhys;
	[NonSerialized] public S_PlayerVelocity      _PlayerVel;
	[NonSerialized] public S_ActionManager       _Actions;
	[NonSerialized] public S_PlayerInput         _Input;
	[NonSerialized] public S_PlayerEvents        _Events;
	[NonSerialized] public S_Handler_Camera        _CamHandler;
	[NonSerialized] public S_Control_SoundsPlayer _Sounds;
	[NonSerialized] public S_PlayerCoreValues	_CoreValues;
	[NonSerialized] public S_PlayerMovement      _PlayerMovement;
	[NonSerialized] public S_ActionChain		_ActionChain;

	[NonSerialized] public Transform             _MainSkin;
	[NonSerialized] public Animator              _CharacterAnimator;
	[NonSerialized] public S_Spawn_UI.StrucCoreUIElements _CoreUIElements;

	public virtual void Awake () {
		if (_PlayerPhys == null)
		{
			AssignTools();
			AssignStats();
		}
	}

	public virtual void AssignTools () {

		if(!_Tools) _Tools = GetComponentInParent<S_CharacterTools>();
		if (!_Tools) _Tools = GetComponent<S_CharacterTools>();

		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		_CamHandler = _Tools.CamHandler;
		_Actions = _Tools._ActionManager;
		_Events = _Tools.PlayerEvents;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_CoreValues = _Tools._CoreValues;
		_PlayerMovement = _Tools._PlayerMove;
		_Sounds = _Tools.SoundControl;
		_ActionChain = _Tools.GetComponent<S_ActionChain>();

		_MainSkin = _Tools.MainSkin;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_CoreUIElements = _Tools.UISpawner._BaseUIElements;
	}

	public virtual void AssignStats () {

	}
}
