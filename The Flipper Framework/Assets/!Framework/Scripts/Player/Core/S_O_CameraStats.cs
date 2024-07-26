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
		[Tooltip("If true, when moving, camera will move up/down to a set height")]
		public bool LockHeight;
		[Tooltip("How fast the camera will move to this height. Units per second")]
		public float LockHeightSpeed;
		[Tooltip("The height to move towards. Calculated in units representing the angle, where 90 is directly above, - 90 is directly below, both facing inwards at the character.")]
		public float HeightToLock;
		[Tooltip("How many seconds after changing the camera height until this takes effect.")]
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
		[Tooltip("If true, the camera will look directly downwards from above the character if in the air for long enough, making landings easier.")]
		public bool shouldLookDownWhenInAir;
		[Tooltip("How high the character needs to be above the ground for this to be considered.")]
		public float minHeightToLookDown ;
		[Tooltip("How fast the player must be falling before this takes effect. Should be negative")]
		public float FallSpeedThreshold ;
		[Tooltip("How quickly to rotate to face down when in effect.")]
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
			CameraDistance = 8,
			shouldAffectDistancebySpeed = true,
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
		[Tooltip("How far the camera should be from the character, will be less if there is a solid object.")]
		public float CameraDistance;
		[Tooltip("If true, the camera distance will change depending on running speed.")]
		public bool shouldAffectDistancebySpeed;
		[Tooltip("Multiply distance by the x, obtained from current running speed.")]
		public AnimationCurve cameraDistanceBySpeed;
		[Tooltip("Objects on this layer will force the camera closer to the player if blocking it from reaching its proper distance.")]
		public LayerMask CollidableLayers;
	}
	#endregion

	#region FOV
	//-------------------------------------------------------------------------------------------------

	public StrucFOV DefaultFOVStats = SetStrucFOV();
	public StrucFOV FOVStats = SetStrucFOV();

	static StrucFOV SetStrucFOV () {
		return new StrucFOV
		{
			baseFOV = 50,
			shouldAffectFOVbySpeed = true,

		};
	}

	[System.Serializable]
	public struct StrucFOV
	{
		[Tooltip("How far the camera should be from the character, will be less if there is a solid object.")]
		public float baseFOV;
		[Tooltip("If true, the camera FOV will change depending on running speed.")]
		public bool shouldAffectFOVbySpeed;
		[Tooltip("Multiply FOV by the x, obtained from current running speed.")]
		public AnimationCurve cameraFOVBySpeed;
	}
	#endregion

	#region cinemachine
	//-------------------------------------------------------------------------------------------------

	public StrucCinemachine DefaultCinemachineStats = SetStruccinemachine();
	public StrucCinemachine cinemachineStats = SetStruccinemachine();

	static StrucCinemachine SetStruccinemachine () {
		return new StrucCinemachine
		{
			dampingBehind = new Vector3(0.2f, 0, 0.35f),
			dampingInFront = new Vector3(0.1f, 0, 0.1f),
			softZone = new Vector2(0.25f, 0.5f),
			deadZone = new Vector2(0, 0.1f),

		};
	}

	[System.Serializable]
	public struct StrucCinemachine
	{
		[Tooltip("How the camera will move towards target location on X, Y, Z values. This is how it will be when the character is facing away from the camera.")]
		public Vector3      dampingBehind;
		[Tooltip("The same as above, but these stats will be used when the character is facing towards the camera.")]
		public Vector3      dampingInFront;
		[Tooltip("The boundaries on the screen where the camera will slowly to move to keep the target within.")]
		public Vector2      softZone;
		[Tooltip("Boundaries on the screen where the camera will not adjust unless the target goes outside of them.")]
		public Vector2      deadZone;
	}
	#endregion

	#region aligning
	//-------------------------------------------------------------------------------------------------

	public StrucAligning DefaultAligningStats = SetStrucaligning();
	public StrucAligning AligningStats = SetStrucaligning();

	static StrucAligning SetStrucaligning () {
		return new StrucAligning
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
	public struct StrucAligning
	{
		[Tooltip("The upwards direction the character needs to be at before the camera starts rotating to match the character rotation. 1 = flat ground so always, less than -1 means it won't happen.")]
		public float	angleThresholdUpwards;
		[Tooltip("The upwards direction the character needs to be at before the applied rotation matching the character's ends and the camera returns to normal.")]
		public float	angleThresholdDownwards;
		[Tooltip("How quickly to rotate to match the character.")]
		public float        CameraVerticalRotationSpeed;
		[Tooltip("Increase the rotations speed by the current angle difference that frame.")]
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
		[Tooltip("If true, the target the camera follows will be offset away from the character in the direction the player in inputting.")]
		public bool                   shouldMoveInInputDirection;
		[Tooltip("How far away this offset can reach.")]
		public float		inputPredictonDistance;
		[Tooltip("How quickly this offset will reflect the player's current input.")]
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
		[Tooltip("How quickly the camera will rotate around the character horizontally at max input.")]
		public float InputXSpeed;
		[Tooltip("How quickly the camera will rotate up and down the character at max input.")]
		public float InputYSpeed;
		[Tooltip("If using controller, speed is multiplied by this.")]
		public float InputSensi;
		[Tooltip("If using mouse and keyboard, camera speed is multiplied by this.")]
		public float InputMouseSensi;
		[Tooltip("If the character is not moving, camera will rotate multiplied by this.")]
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
		[Tooltip("If true, the target the camera follows will be offset from the character based on the angle difference between character facing direction and camera facing direction.")]
		public bool                  shouldMoveBasedOnAngle;
		[Tooltip("How much upwards to offset the camera based on the the angle difference from -1 to 1 (-90 to 90)")]
		public AnimationCurve        moveUpByAngle;
		[Tooltip("How much to the right to offset the camera based on the the angle difference from -1 to 1 (-90 to 90), 0x means directily behind character.")]
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
		[Tooltip("When running faster than this speed, the camera will keep rotating back to face the same direction as the character.")]
		public float LockCamAtHighSpeed;
		[Tooltip("How many seconds after changing the horizontal angle before this takes effect.")]
		public float afterMoveXDelay;
		[Tooltip("How many degrees per second to rotate behind the character.")]
		public float rotateToBehindSpeed ;
		[Tooltip("How much the player needs to rotate in degrees before the camera will start to rotate to behind it. 90 = the character must turn 90 degrees before the camera starts rotating until directly behind, where this is reset.")]
		public float rotateCharacterBeforeCameraFollows;
		[Tooltip("If not currently rotating behind character, this determines how quickly an invisibile checker rotates to match the character, which is used in determening the above. The lower, the longer the character can turn for.")]
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
		[Tooltip("The camera can not rotate downwards beyond this angle. -90 = looking straight above from below.")]
		public float yMinLimit;
		[Tooltip("The camera can not rotate upwards beyond this angle. 90 = looking straight down from above.")]
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
		[Tooltip("All shake effects will be multiplied by this. 0.5 = all shake will be diminished by half.")]
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
		DrawFOV();
		DrawCinemachine();
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

		//FOV
		#region FOV
		void DrawFOV () {
			EditorGUILayout.Space();
			DrawProperty("FOVStats", "Camera FOV");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.FOVStats = stats.DefaultFOVStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//cinemachine
		#region cinemachine
		void DrawCinemachine () {
			EditorGUILayout.Space();
			DrawProperty("cinemachineStats", "Cinemachine framing transposer");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.cinemachineStats = stats.DefaultCinemachineStats;
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