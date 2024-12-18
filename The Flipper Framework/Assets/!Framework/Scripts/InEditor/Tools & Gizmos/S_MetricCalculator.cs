using System.Collections;
using System.Collections.Generic;
using templates;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
[RequireComponent(typeof(S_PlayerPhysics))]
[RequireComponent(typeof(S_CharacterTools))]
public class S_MetricCalculator : MonoBehaviour
{
	public S_O_CharacterStats	_Stats;
	public S_O_CameraStats	_CamStats;

	public S_PlayerPhysics _PlayerPhys;
	public S_CharacterTools _Tools;

	[ExecuteAlways]
	private void OnEnable () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Tools = GetComponent<S_CharacterTools>();
	}


	public void CalculateAll () {

	}

	//Metrics To Calculate
	#region MetricsToCalculate

	#region Speeds
	public void CalculateTopSpeed () {

	}

	public void CalculateMaxSpeed () {

	}

	public void CalculateTimeToXSpeedFromYSpeed () {

	}

	public void CalculateTimeToXSpeedOnYAngle () {

	}

	public void CalculateTimeFromXSpeedToZeroOnYAngle () {

	}
	#endregion

	#region ground
	public void CalculateAngleToStartFollowCamera () {

	}

	public void CalculateAngleToStartSlope () {

	}
	#endregion

	#region character
	
	#endregion

	#region jumps

	public void CalculateJumpHeight () {

	}

	public void CalculateDoubleJumpHeight () {

	}

	public void CalculateJumpCurve () {

	}

	public void CalculateJumpDistanceAtTopSpeed () {

	}

	public void CalculateJumpDistanceAtMaxSpeed () {

	}

	public void CalculateTimeToFallXHeight () {

	}

	public void CalculateDistanceFellInXTimeFromYSpeed () {

	}
	#endregion

	#region sitActions

	public void CalculateRingRoadSpeed () {

	}

	public void CalculateSpeedAfterRingRoadFromXSpeed () {

	}
	#endregion

	#region Camera

	public void CalculateCameraMinDistance () {

	}

	public void CalculateCameraMaxDistance () {

	}
	#endregion

	#region air Actions

	public void CalculateHomingDistanceRadius () {

	}

	public void CalculateHomingDistanceFromCam () {

	}

	public void CalculateTimeToFullDropCharge () {

	}

	public void CalculateDistanceFellAtXSpeedWhenDropCharging () {

	}
	#endregion

	#region groundActions

	public void CalculateDistanceToStopWhenRollingFromXSpeed () {

	}

	public void CalculateDistanceToStopWhenSpinChargingFromXSpeed () {

	}

	public void CalculateTimeToFullSpinCharge () {

	}
	#endregion

	#endregion

	public S_O_CustomInspectorStyle _InspectorTheme;
}



[CustomEditor(typeof(S_MetricCalculator))]
public class MetricCalculatorEditor : S_CustomInspector_Base
{
	S_MetricCalculator _OwnerScript;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_MetricCalculator)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		//Describe what the script does
		EditorGUILayout.TextArea("Details.", EditorStyles.textArea);

		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject, "_Stats", "Stats");

		//Add new element button.
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Calculate All", _BigButtonStyle, _OwnerScript, "Calculate All"))
		{
			_OwnerScript.CalculateAll();
		}
	}
}
#endif

