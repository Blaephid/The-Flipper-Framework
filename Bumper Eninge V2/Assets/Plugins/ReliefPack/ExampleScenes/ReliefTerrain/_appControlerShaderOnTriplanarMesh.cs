using UnityEngine;
using System.Collections;

public class _appControlerShaderOnTriplanarMesh : MonoBehaviour {
	public bool shadows=false;
	public bool forward_path=true;
	private bool panel_enabled;
	//public float light_dir=0;
	public float model_dir=0;
	
	void Awake() {
		panel_enabled=true;
	}
	
	void Update() {
		if (Input.GetKeyDown(KeyCode.P)) {
			panel_enabled=!panel_enabled;
		}
	}
	
	void OnGUI () {
		GUILayout.Space(10);
		GUILayout.BeginVertical("box");

		GUILayout.Label (""+FPSmeter.fps);
		if (panel_enabled) {
			shadows = GUILayout.Toggle(shadows, "disable Unity's shadows");
		 	Light light=GameObject.Find("Directional light").GetComponent<Light>() as Light;
			light.shadows=shadows ?  LightShadows.None : LightShadows.Soft;
			
			forward_path = GUILayout.Toggle(forward_path, "forward rendering");
		 	Camera cam=GameObject.Find("Main Camera").GetComponent<Camera>() as Camera;
			cam.renderingPath = forward_path ?  RenderingPath.Forward : RenderingPath.DeferredShading;

            //GUILayout.Label ("Light", GUILayout.MaxWidth(40));
            //light_dir = GUILayout.HorizontalSlider(light_dir, 0, 360);
            //light.transform.rotation=Quaternion.Euler(light_dir,light_dir,light_dir);

            //if (!Application.isWebPlayer) {
            //	if (GUILayout.Button("QUIT")) {
            //		Application.Quit();
            //	}
            //}
            GUILayout.Label("  Drag model/env to rotate");
            GUILayout.Label("  Wheel - zoom camera");
        } else {
			//if (!Application.isWebPlayer) {
			//	if (GUILayout.Button("QUIT")) {
			//		Application.Quit();
			//	}
			//}
		}
		GUILayout.Label ("  P - toggle panel");
		GUILayout.EndVertical();
	}
}
