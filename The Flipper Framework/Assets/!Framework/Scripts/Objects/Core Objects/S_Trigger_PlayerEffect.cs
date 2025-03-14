using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class S_Trigger_PlayerEffect : S_Trigger_External
{

	public S_Trigger_PlayerEffect () {
		_isLogicInPlayerScript = true;
	}

	[Header("Interaction")]
	[SetBoolIfOther(true, "_ActionsToDisable", 0), SetBoolIfOther(true, "_SubActionsToDisable", 0)]
	public float	_framesBeforeDeactivate = 0;

	[Header("Input")]
	[Range(-1, 100)]
	public int		_lockPlayerInputFor = 5;
	public S_GeneralEnums.LockControlDirection _LockInputTo_;
	public Vector3          _lockToDirectionIfChange;

	[Header("Actions")]
	public S_S_ActionHandling.PrimaryPlayerStates[]		_ActionsToDisable;
	public S_S_ActionHandling.SubPlayerStates[]		_SubActionsToDisable;

	[Header("Effects")]
	[Tooltip("In case the player needs to be in a specific state. Mainly used to call on Grounded events while still in the air. E.G. Returning jump dash in scripted sections.")]
	public S_GeneralEnums.ChangeGroundedState      _setPlayerGrounded;

	public ValueEditing[] _EditValues;


#if UNITY_EDITOR
	public override void DrawAdditionalGizmos (bool selected, Color colour ) {
		base.DrawAdditionalGizmos(selected, colour );
	
	}
#endif
}

[Serializable]
public class ValueEditing
{
	public string ComponentName = "S_PlayerPhysics";
	public string valueName;
	[Space]
	public DataTypes DataType;

	[HideInInspector] public object rememberValueObject;
	[HideInInspector] public UnityEngine.Component rememberComponent;
	[HideInInspector] public FieldInfo rememberField;

	[OnlyDrawIf("DataType", DataTypes.Float)]
	public float replaceFloat;
	[HideInInspector] public float rememberFloat;

	[OnlyDrawIf("DataType", DataTypes.Vector3)]
	public Vector3 replaceVector3;
	[HideInInspector] public Vector3 rememberVector3;

	[OnlyDrawIf("DataType", DataTypes.Vector2)]
	public Vector2 replaceVector2;
	[HideInInspector] public Vector2 rememberVector2;

	[OnlyDrawIf("DataType", DataTypes.String)]
	public string replaceString;
	[HideInInspector] public string rememberString;

	[OnlyDrawIf("DataType", DataTypes.Boolean)]
	public bool replaceBool;
	[HideInInspector] public bool rememberBool;


	[OnlyDrawIf("DataType", DataTypes.Int)]
	public int replaceInt;
	[HideInInspector] public int rememberInt;
}
