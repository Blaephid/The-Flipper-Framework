using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Camera X Stats")]
public class S_O_CameraStats : ScriptableObject
{
	[HideInInspector] public string Title = "Title";

	#region lockHeight
	//-------------------------------------------------------------------------------------------------

	public StruclockHeight DefaultLockHeightStats = SetStruclockHeight();
	public StruclockHeight LockHeightStats = SetStruclockHeight();

	static StruclockHeight SetStruclockHeight () {
		return new StruclockHeight
		{
			LockHeight = true,
			LockHeightSpeed = 0.8f,
			HeightToLock = 10,	
			afterMoveYDelay = 0.4f,
		};
	}

	[System.Serializable]
	public struct StruclockHeight
	{
		public bool LockHeight;
		public float LockHeightSpeed;
		public float HeightToLock;
		public float afterMoveYDelay;

	}
	#endregion

	#region autoLookDown
	//-------------------------------------------------------------------------------------------------

	public StrucAutoLookDown DefaultAutoLookDownStats = SetStrucautoLookDown();
	public StrucAutoLookDown AutoLookDownStats = SetStrucautoLookDown();

	static StrucAutoLookDown SetStrucautoLookDown () {
		return new StrucAutoLookDown
		{
			shouldLookDownWhenInAir = true,
			minHeightToLookDown = 50,
			FallSpeedThreshold =-140,
			HeightFollowSpeed = 6,
		};
	}

	[System.Serializable]
	public struct StrucAutoLookDown
	{
		public bool shouldLookDownWhenInAir;
		public float minHeightToLookDown ;
		public float FallSpeedThreshold ;
		public float HeightFollowSpeed ;

	}
	#endregion

	#region distance
	//-------------------------------------------------------------------------------------------------

	public StrucDistance DefaultDistanceStats = SetStrucDistance();
	public StrucDistance DistanceStats = SetStrucDistance();

	static StrucDistance SetStrucDistance () {
		return new StrucDistance
		{
			CameraMaxDistance = 8,
			cameraDistanceBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0.8f),
				new Keyframe(0.2f, 0.85f),
				new Keyframe(0.3f, 1f),
				new Keyframe(0.4f, 1f),
				new Keyframe(0.6f, 1.2f),
				new Keyframe(0.7f, 1.2f),
				new Keyframe(0.8f, 1.35f),
				new Keyframe(0.85f, 1.5f),
				new Keyframe(1f, 1.5f),
			}),
			CollidableLayers = new LayerMask()
		};
	}

	[System.Serializable]
	public struct StrucDistance
	{
		public float CameraMaxDistance;
		public AnimationCurve cameraDistanceBySpeed;
		public LayerMask CollidableLayers;
	}
	#endregion

	#region aligning
	//-------------------------------------------------------------------------------------------------

	public Strucaligning DefaultAligningStats = SetStrucaligning();
	public Strucaligning AligningStats = SetStrucaligning();

	static Strucaligning SetStrucaligning () {
		return new Strucaligning
		{
			angleThresholdDownwards = 0.8f,
			angleThresholdUpwards = 0.4f,
			CameraVerticalRotationSpeed = 11,
			vertFollowSpeedByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1f),
				new Keyframe(1f, 1.5f),
			}),
		};
	}

	[System.Serializable]
	public struct Strucaligning
	{
		public float	angleThresholdUpwards;
		public float	angleThresholdDownwards;
		public float        CameraVerticalRotationSpeed;
		public AnimationCurve         vertFollowSpeedByAngle;
	}
	#endregion

	#region targetLookAhead
	//-------------------------------------------------------------------------------------------------

	public StructargetLookAhead DefaultLookAheadStats = SetStructargetLookAhead();
	public StructargetLookAhead LookAheadStats = SetStructargetLookAhead();

	static StructargetLookAhead SetStructargetLookAhead () {
		return new StructargetLookAhead
		{
			shouldMoveInInputDirection = true,
			inputPredictonDistance = 1.5f,
			cameraMoveToInputSpeed = 3,
		};
	}

	[System.Serializable]
	public struct StructargetLookAhead
	{
		public bool                   shouldMoveInInputDirection;
		public float		inputPredictonDistance;
		public float		cameraMoveToInputSpeed;
	}
	#endregion

	#region targetinput
	//-------------------------------------------------------------------------------------------------

	public StrucInput DefaultInputStats = SetStrucInput();
	public StrucInput InputStats = SetStrucInput();

	static StrucInput SetStrucInput () {
		return new StrucInput
		{
			InputXSpeed = 70,
			InputYSpeed = 55,
			InputSensi = 1,
			InputMouseSensi = 0.035f,
			stationaryCamIncrease = 1.35f
		};
	}

	[System.Serializable]
	public struct StrucInput
	{
		public float InputXSpeed;
		public float InputYSpeed;
		public float InputSensi;
		public float InputMouseSensi;
		public float stationaryCamIncrease;
	}
	#endregion

	#region targetByAngle
	//-------------------------------------------------------------------------------------------------

	public StructargetByAngle DefaultTargetByAngleStats = SetStructargetByAngle();
	public StructargetByAngle TargetByAngleStats = SetStructargetByAngle();

	static StructargetByAngle SetStructargetByAngle () {
		return new StructargetByAngle
		{
			shouldMoveBasedOnAngle = true,
			moveUpByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(-1, 1f),
				new Keyframe(-0.7f, 1f),
				new Keyframe(-0.35f, 0f),
				new Keyframe(0.35f, 0f),
				new Keyframe(0.75f, 1.6f),
				new Keyframe(1f, 1.6f),
			}),
			moveSideByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(-1, -1.5f),
				new Keyframe(-0.2f, 0f),
				new Keyframe(0.2f, 0f),
				new Keyframe(1f, 1.5f),
			}),
		};
	}

	[System.Serializable]
	public struct StructargetByAngle
	{
		public bool                  shouldMoveBasedOnAngle;
		public AnimationCurve        moveUpByAngle;
		public AnimationCurve        moveSideByAngle;
	}
	#endregion

	#region RotateBehind
	//-------------------------------------------------------------------------------------------------

	public StrucRotateBehind DefaultRotateBehindStats = SetStrucRotateBehind();
	public StrucRotateBehind RotateBehindStats = SetStrucRotateBehind();

	static StrucRotateBehind SetStrucRotateBehind () {
		return new StrucRotateBehind
		{
			LockCamAtHighSpeed = 90,
			afterMoveXDelay = 0.4f,
			rotateToBehindSpeed = 5,
			rotateCharacterBeforeCameraFollows = 0.2f,
			followFacingDirectionSpeed = 0.5f,
		};
	}

	[System.Serializable]
	public struct StrucRotateBehind
	{
		public float LockCamAtHighSpeed;
		public float afterMoveXDelay;
		public float rotateToBehindSpeed ;
		public float rotateCharacterBeforeCameraFollows;
		public float followFacingDirectionSpeed;
	}
	#endregion

	#region Clamping
	//-------------------------------------------------------------------------------------------------

	public StrucClamping DefaultClampingStats = SetStrucClamping();
	public StrucClamping ClampingStats = SetStrucClamping();

	static StrucClamping SetStrucClamping () {
		return new StrucClamping
		{
			yMaxLimit = 90,
			yMinLimit = -90,
		};
	}

	[System.Serializable]
	public struct StrucClamping
	{
		public float yMinLimit;
		public float yMaxLimit ;
	}
	#endregion

	#region Effects
	//-------------------------------------------------------------------------------------------------

	public StrucEffects DefaultEffectsStats = SetStrucEffects();
	public StrucEffects EffectsStats = SetStrucEffects();

	static StrucEffects SetStrucEffects () {
		return new StrucEffects
		{
			ShakeDampen = 4,
		};
	}

	[System.Serializable]
	public struct StrucEffects
	{
		public float ShakeDampen;
	}
	#endregion

	public S_O_CustomInspectorStyle InspectorTheme;
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_CameraStats))]
public class S_O_CameraStatsEditor : Editor
{
	S_O_CameraStats stats;
	GUIStyle headerStyle;
	GUIStyle ResetToDefaultButton;

	public override void OnInspectorGUI () {
		DrawInspector();
	}
	private void OnEnable () {
		//Setting variables
		stats = (S_O_CameraStats)target;

		if (stats.InspectorTheme == null) { return; }
		headerStyle = stats.InspectorTheme._MainHeaders;
		ResetToDefaultButton = stats.InspectorTheme._ResetButton;
	}

	private void DrawInspector () {
		EditorGUILayout.PropertyField(serializedObject.FindProperty("InspectorTheme"), new GUIContent("Inspector Theme"));
		serializedObject.ApplyModifiedProperties();

		//Will only happen if above is attatched.
		if (stats == null) return;

		serializedObject.Update();

		//Start Tite and description
		stats.Title = EditorGUILayout.TextField(stats.Title);

		EditorGUILayout.TextArea("This objects contains a bunch of stats you can change to adjust how the camera controls. \n", EditorStyles.textArea);

		DrawInput();

		DrawDistance();
		DrawAligning();
		DrawLockHeight();
		DrawRotateBehind();
		DrawAutoLookDown();

		DrawTargetByAngle();
		DrawLookAhead();
		DrawEffects();
		DrawClamping();


		void DrawProperty ( string property, string outputName ) {
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
		}

		//Distance
		#region Distance
		void DrawDistance () {
			EditorGUILayout.Space();
			DrawProperty("DistanceStats", "Camera Distance");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.DistanceStats = stats.DefaultDistanceStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//aligning
		#region aligning
		void DrawAligning () {
			EditorGUILayout.Space();
			DrawProperty("AligningStats", "Aligning Camera to Character");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.AligningStats = stats.DefaultAligningStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//RotateBehind
		#region RotateBehind
		void DrawRotateBehind () {
			EditorGUILayout.Space();
			DrawProperty("RotateBehindStats", "Rotate Behind When Moving");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.RotateBehindStats = stats.DefaultRotateBehindStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//LockHeight
		#region LockHeight
		void DrawLockHeight () {
			EditorGUILayout.Space();
			DrawProperty("LockHeightStats", "Reset Height When Moving");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.LockHeightStats = stats.DefaultLockHeightStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//AutoLookDown
		#region AutoLookDown
		void DrawAutoLookDown () {
			EditorGUILayout.Space();
			DrawProperty("AutoLookDownStats", "Look Down In Air");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.AutoLookDownStats = stats.DefaultAutoLookDownStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//LookAhead
		#region LookAhead
		void DrawLookAhead () {
			EditorGUILayout.Space();
			DrawProperty("LookAheadStats", "Look Ahead");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.LookAheadStats = stats.DefaultLookAheadStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//TargetByAngle
		#region TargetByAngle
		void DrawTargetByAngle () {
			EditorGUILayout.Space();
			DrawProperty("TargetByAngleStats", "Moving Target By Angle");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.TargetByAngleStats = stats.DefaultTargetByAngleStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Input
		#region Input
		void DrawInput () {
			EditorGUILayout.Space();
			DrawProperty("InputStats", "Input");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.InputStats = stats.DefaultInputStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Clamping
		#region Clamping
		void DrawClamping () {
			EditorGUILayout.Space();
			DrawProperty("ClampingStats", "Clamping");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.ClampingStats = stats.DefaultClampingStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Effects
		#region Effects
		void DrawEffects () {
			EditorGUILayout.Space();
			DrawProperty("EffectsStats", "Effects");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.EffectsStats = stats.DefaultEffectsStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion
	}
}
#endif