using UnityEngine;
using System.Collections;

public class skyboxspace_demo_v1 : MonoBehaviour
{

	public Material[] skyBoxMaterial;
	public Vector3[] sunPosition;
	private int skyBoxLength = 0;
	private int currentSkyBoxIndex = 0;

	public string topText;

	private float counter;

	private int frames;

	private float fps;

	private static GUIStyle whiteStyle;

	private static GUIStyle blackStyle;

	public GameObject sun;

	// Use this for initialization
	void Start ()
	{
		RenderSettings.skybox = skyBoxMaterial [0]; 
		skyBoxLength = skyBoxMaterial.Length;
		topText = skyBoxMaterial [currentSkyBoxIndex].name;
		sun.transform.eulerAngles = sunPosition[0]; 

	}

	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.F))
			Screen.fullScreen = !Screen.fullScreen;


		if (Input.GetKeyDown (KeyCode.N)) {
			currentSkyBoxIndex++;
			if (currentSkyBoxIndex >= skyBoxLength) {
				currentSkyBoxIndex = 0;
			}
			RenderSettings.skybox = skyBoxMaterial [currentSkyBoxIndex]; 
			topText = skyBoxMaterial [currentSkyBoxIndex].name;
			sun.transform.eulerAngles = sunPosition[currentSkyBoxIndex]; 

		}

		// FPS 
		counter += Time.deltaTime;
		frames += 1;

		if (counter >= 1.0f) {
			fps = (float)frames / counter;

			counter = 0.0f;
			frames = 0;
		}


	
	}

	protected virtual void OnGUI ()
	{

		if (fps > 0.0f) {
			DrawText ("FPS: " + fps.ToString ("0"), TextAnchor.UpperLeft);
		}

		if (string.IsNullOrEmpty (topText) == false) {
			DrawText ("Skybox[" + currentSkyBoxIndex + "] Name:" + topText + " (Press the [N] key for the next skybox)", TextAnchor.UpperCenter, 150);
		}



	}

	private static void DrawText (string text, TextAnchor anchor, int offsetX = 15, int offsetY = 15)
	{
		if (string.IsNullOrEmpty (text) == false) {
			if (whiteStyle == null || blackStyle == null) {
				whiteStyle = new GUIStyle ();
				whiteStyle.fontSize = 20;
				whiteStyle.fontStyle = FontStyle.Bold;
				whiteStyle.wordWrap = true;
				whiteStyle.normal = new GUIStyleState ();
				whiteStyle.normal.textColor = Color.white;

				blackStyle = new GUIStyle ();
				blackStyle.fontSize = 20;
				blackStyle.fontStyle = FontStyle.Bold;
				blackStyle.wordWrap = true;
				blackStyle.normal = new GUIStyleState ();
				blackStyle.normal.textColor = Color.black;
			}

			whiteStyle.alignment = anchor;
			blackStyle.alignment = anchor;

			var sw = (float)Screen.width;
			var sh = (float)Screen.height;
			var rect = new Rect (0, 0, sw, sh);

			rect.xMin += offsetX;
			rect.xMax -= offsetX;
			rect.yMin += offsetY;
			rect.yMax -= offsetY;

			rect.x += 1;
			GUI.Label (rect, text, blackStyle);

			rect.x -= 2;
			GUI.Label (rect, text, blackStyle);

			rect.x += 1;
			rect.y += 1;
			GUI.Label (rect, text, blackStyle);

			rect.y -= 2;
			GUI.Label (rect, text, blackStyle);

			rect.y += 1;
			GUI.Label (rect, text, whiteStyle);
		}
	}
}
