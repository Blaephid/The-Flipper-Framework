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


	
	public bool LockHeight = true;
	public float LockHeightSpeed = 0.8f;
	public bool MoveHeightBasedOnSpeed = true;
	public float minHeightToLookDown = 50;
	public float HeightToLock = 15;
	public float HeightFollowSpeed = 6;
	public float FallSpeedThreshold = -80;

	public float CameraMaxDistance = 15;
	public AnimationCurve cameraDistanceBySpeed;
	public Vector2 AngleThreshold = new Vector2 (0.3f, 0.2f);

	public LayerMask CollidableLayers;

	public Vector2               softZone;
	public Vector2               deadZone;
	public Vector2               turnZone;
	public bool                   shouldMoveInInputDirection = true;

	public float CameraRotationSpeed = 100;
	public float CameraVerticalRotationSpeed = 12;
	public AnimationCurve vertFollowSpeedByAngle;
	public float CameraMoveSpeed = 100;

	public float InputXSpeed = 70;
	public float InputYSpeed = 55f;
	public float InputSensi = 1f;
	public float InputMouseSensi = 0.06f;
	public float stationaryCamIncrease = 1.3f;

	public float afterMoveXDelay = 0.3f;
	public float afterMoveYDelay = 0.5f;

	public float yMinLimit = -100f;
	public float yMaxLimit = 100f;

	public float LockCamAtHighSpeed = 45;

	public float LockedRotationSpeed = 6;
	public float ShakeDampen = 4;

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
		ResetToDefaultButton = stats.InspectorTheme._DefaultButton;
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

		DrawDefaultInspector();


		void DrawProperty ( string property, string outputName ) {
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
		}

		//Speeds
		#region Speeds
		void DrawDistance () {
			EditorGUILayout.Space();
			DrawProperty("CameraMaxDistance", "Distance");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				//stats.SpeedStats = stats.DefaultSpeedStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion
	}
}
#endif