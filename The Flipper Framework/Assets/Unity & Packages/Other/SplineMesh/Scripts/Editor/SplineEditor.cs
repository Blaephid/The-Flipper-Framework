using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SplineMesh
{
	[CustomEditor(typeof(Spline))]
	public class SplineEditor : Editor
	{

		private const int QUAD_SIZE = 12;
		private static Color CURVE_COLOR = new Color(0.8f, 0.8f, 0.8f);
		private static Color CURVE_BUTTON_COLOR = new Color(0.8f, 0.8f, 0.8f);
		private static Color DIRECTION_COLOR = Color.red;
		private static Color DIRECTION_BUTTON_COLOR = Color.red;
		private static Color UP_BUTTON_COLOR = Color.green;

		private static bool showUpVector = false;

		private enum SelectionType
		{
			Node,
			Direction,
			InverseDirection,
			Up
		}

		private SplineNode currentSelection 
			{ get { return selectionBackerField;} 
			set { Debug.Log("Set to " + (value == null ? "null" : value)); selectionBackerField = value; } }
		private SplineNode selectionBackerField;

		private SelectionType selectionType;
		private bool mustCreateNewNode = false;
		private SerializedProperty nodesProp { get { return serializedObject.FindProperty("nodes"); } }

		private Spline _Spline { get { return (Spline)serializedObject.targetObject; } }

		private GUIStyle nodeButtonStyle, directionButtonStyle, upButtonStyle;

		private void OnEnable () {
			Texture2D t = new Texture2D(1, 1);
			t.SetPixel(0, 0, CURVE_BUTTON_COLOR);
			t.Apply();
			nodeButtonStyle = new GUIStyle();
			nodeButtonStyle.normal.background = t;

			t = new Texture2D(1, 1);
			t.SetPixel(0, 0, DIRECTION_BUTTON_COLOR);
			t.Apply();
			directionButtonStyle = new GUIStyle();
			directionButtonStyle.normal.background = t;

			t = new Texture2D(1, 1);
			t.SetPixel(0, 0, UP_BUTTON_COLOR);
			t.Apply();
			upButtonStyle = new GUIStyle();
			upButtonStyle.normal.background = t;
			currentSelection = null;


			try
			{
				Undo.undoRedoPerformed -= _Spline.RefreshCurves;
				Undo.undoRedoPerformed += _Spline.RefreshCurves;
			}
			catch { }
		}

		SplineNode AddClonedNode ( SplineNode node ) {
			int index = _Spline.nodes.IndexOf(node);
			SplineNode res = new SplineNode(node.Position, node.Direction);
			if (index == _Spline.nodes.Count - 1)
			{
				_Spline.AddNode(res);
			}
			else
			{
				_Spline.InsertNode(index + 1, res);
			}
			return res;
		}

		void OnSceneGUI () {
			// disable game object transform gyzmo
			// if the spline script is active
			if (Selection.activeGameObject == _Spline.gameObject)
			{
				if (!_Spline.enabled)
				{
					Tools.current = Tool.Move;
				}
				else
				{
					Tools.current = Tool.None;
					if (currentSelection == null && _Spline.nodes.Count > 0)
						currentSelection = _Spline.nodes[0];
				}
			}

			// draw a bezier curve for each curve in the spline
			foreach (CubicBezierCurve curve in _Spline.GetCurves())
			{
				Handles.DrawBezier(_Spline.transform.TransformPoint(curve.n1.Position),
				    _Spline.transform.TransformPoint(curve.n2.Position),
				    _Spline.transform.TransformPoint(curve.n1.Direction),
				    _Spline.transform.TransformPoint(curve.GetInverseDirection()),
				    CURVE_COLOR,
				    null,
				    3);
			}

			if (!_Spline.enabled)
				return;
			if (!_Spline || !_Spline.transform || currentSelection == null) { return; }

			// draw the selection handles
			switch (selectionType)
			{
				case SelectionType.Node:
					// place a handle on the node and manage position change

					// TODO place the handle depending on user params (local or world)
					Vector3 newPosition = _Spline.transform.InverseTransformPoint(Handles.PositionHandle(_Spline.transform.TransformPoint(currentSelection.Position), _Spline.transform.rotation));
					if (newPosition != currentSelection.Position)
					{
						// position handle has been moved
						if (mustCreateNewNode)
						{
							mustCreateNewNode = false;
							currentSelection = AddClonedNode(currentSelection);
							currentSelection.Direction += newPosition - currentSelection.Position;
							currentSelection.Position = newPosition;
						}
						else
						{
							currentSelection.Direction += newPosition - currentSelection.Position;
							currentSelection.Position = newPosition;
						}
					}
					break;
				case SelectionType.Direction:
					var result = Handles.PositionHandle(_Spline.transform.TransformPoint(currentSelection.Direction), Quaternion.identity);
					currentSelection.Direction = _Spline.transform.InverseTransformPoint(result);
					break;
				case SelectionType.InverseDirection:
					result = Handles.PositionHandle(2 * _Spline.transform.TransformPoint(currentSelection.Position) - _Spline.transform.TransformPoint(currentSelection.Direction), Quaternion.identity);
					currentSelection.Direction = 2 * currentSelection.Position - _Spline.transform.InverseTransformPoint(result);
					break;
				case SelectionType.Up:
					result = Handles.PositionHandle(_Spline.transform.TransformPoint(currentSelection.Position + currentSelection.Up), Quaternion.LookRotation(currentSelection.Direction - currentSelection.Position));
					currentSelection.Up = (_Spline.transform.InverseTransformPoint(result) - currentSelection.Position).normalized;
					break;
			}

			// draw the handles of all nodes, and manage selection motion
			Handles.BeginGUI();
			foreach (SplineNode n in _Spline.nodes)
			{
				var dir = _Spline.transform.TransformPoint(n.Direction);
				var pos = _Spline.transform.TransformPoint(n.Position);
				var invDir = _Spline.transform.TransformPoint(2 * n.Position - n.Direction);
				var up = _Spline.transform.TransformPoint(n.Position + n.Up);
				// first we check if at least one thing is in the camera field of view
				if (!(CameraUtility.IsOnScreen(pos) ||
				    CameraUtility.IsOnScreen(dir) ||
				    CameraUtility.IsOnScreen(invDir) ||
				    (showUpVector && CameraUtility.IsOnScreen(up))))
				{
					continue;
				}

				Vector3 guiPos = HandleUtility.WorldToGUIPoint(pos);
				if (n == currentSelection)
				{
					Vector3 guiDir = HandleUtility.WorldToGUIPoint(dir);
					Vector3 guiInvDir = HandleUtility.WorldToGUIPoint(invDir);
					Vector3 guiUp = HandleUtility.WorldToGUIPoint(up);

					// for the selected node, we also draw a line and place two buttons for directions
					Handles.color = DIRECTION_COLOR;
					Handles.DrawLine(guiDir, guiInvDir);

					// draw quads direction and inverse direction if they are not selected
					if (selectionType != SelectionType.Node)
					{
						if (Button(guiPos, directionButtonStyle))
						{
							selectionType = SelectionType.Node;
						}
					}
					if (selectionType != SelectionType.Direction)
					{
						if (Button(guiDir, directionButtonStyle))
						{
							selectionType = SelectionType.Direction;
						}
					}
					if (selectionType != SelectionType.InverseDirection)
					{
						if (Button(guiInvDir, directionButtonStyle))
						{
							selectionType = SelectionType.InverseDirection;
						}
					}
					if (showUpVector)
					{
						Handles.color = Color.green;
						Handles.DrawLine(guiPos, guiUp);
						if (selectionType != SelectionType.Up)
						{
							if (Button(guiUp, upButtonStyle))
							{
								selectionType = SelectionType.Up;
							}
						}
					}
				}
				else
				{
					//if player clicks on point on screen where node is
					if (Button(guiPos, nodeButtonStyle))
					{
						currentSelection = n;
						selectionType = SelectionType.Node;
					}
				}
			}
			Handles.EndGUI();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}

		bool Button ( Vector2 position, GUIStyle style ) {
			return GUI.Button(new Rect(position - new Vector2(QUAD_SIZE / 2, QUAD_SIZE / 2), new Vector2(QUAD_SIZE, QUAD_SIZE)), GUIContent.none, style);
		}

		public override void OnInspectorGUI () {
			serializedObject.Update();

			//if (_Spline.nodes.IndexOf(selection) < 0)
			//{
			//	selection = null;
			//}

			// add button
			if (currentSelection == null)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Add node after selected"))
			{
				Undo.RecordObject(_Spline, "add spline node");
				SplineNode newNode = new SplineNode(currentSelection.Direction, currentSelection.Direction + currentSelection.Direction - currentSelection.Position);
				var index = _Spline.nodes.IndexOf(currentSelection);
				if (index == _Spline.nodes.Count - 1)
				{
					_Spline.AddNode(newNode);
				}
				else
				{
					_Spline.InsertNode(index + 1, newNode);
				}
				currentSelection = newNode;
				serializedObject.Update();
			}
			GUI.enabled = true;

			// delete button
			if (currentSelection == null || _Spline.nodes.Count <= 2)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Delete selected node"))
			{
				Undo.RecordObject(_Spline, "delete spline node");
				_Spline.RemoveNode(currentSelection);
				currentSelection = null;
				serializedObject.Update();
			}
			GUI.enabled = true;

			showUpVector = GUILayout.Toggle(showUpVector, "Show up vector");
			_Spline.IsLoop = GUILayout.Toggle(_Spline.IsLoop, "Is loop");

			// nodes
			GUI.enabled = false;
			EditorGUILayout.PropertyField(nodesProp);
			GUI.enabled = true;

			if (currentSelection != null)
			{
				int index = _Spline.nodes.IndexOf(currentSelection);
				SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(index);

				EditorGUILayout.LabelField("Selected node (node " + index + ")");

				EditorGUI.indentLevel++;
				DrawNodeData(nodeProp, currentSelection);
				EditorGUI.indentLevel--;
			}
			else
			{
				EditorGUILayout.LabelField("No selected node");
			}
		}

		private void DrawNodeData ( SerializedProperty nodeProperty, SplineNode node ) {
			var positionProp = nodeProperty.FindPropertyRelative("position");
			var directionProp = nodeProperty.FindPropertyRelative("direction");
			var upProp = nodeProperty.FindPropertyRelative("up");
			var scaleProp = nodeProperty.FindPropertyRelative("scale");
			var rollProp = nodeProperty.FindPropertyRelative("roll");

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(positionProp, new GUIContent("Position"));
			EditorGUILayout.PropertyField(directionProp, new GUIContent("Direction"));
			EditorGUILayout.PropertyField(upProp, new GUIContent("Up"));
			EditorGUILayout.PropertyField(scaleProp, new GUIContent("Scale"));
			EditorGUILayout.PropertyField(rollProp, new GUIContent("Roll"));

			if (EditorGUI.EndChangeCheck())
			{
				node.Position = positionProp.vector3Value;
				node.Direction = directionProp.vector3Value;
				node.Up = upProp.vector3Value;
				node.Scale = scaleProp.vector2Value;
				node.Roll = rollProp.floatValue;
				serializedObject.Update();
			}
		}

		[MenuItem("GameObject/3D Object/Spline")]
		public static void CreateSpline () {
			new GameObject("Spline", typeof(Spline));
		}

		[DrawGizmo(GizmoType.InSelectionHierarchy)]
		static void DisplayUnselected ( Spline spline, GizmoType gizmoType ) {
			foreach (CubicBezierCurve curve in spline.GetCurves())
			{
				Handles.DrawBezier(spline.transform.TransformPoint(curve.n1.Position),
				    spline.transform.TransformPoint(curve.n2.Position),
				    spline.transform.TransformPoint(curve.n1.Direction),
				    spline.transform.TransformPoint(curve.GetInverseDirection()),
				    CURVE_COLOR,
				    null,
				    3);
			}
		}
	}
}
