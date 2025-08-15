using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class S_TitleScreenControl : MonoBehaviour
{

	[Header("Animations")]
	public Animator     _Measurers;
	public int          _delayBeforeMeasurersEnter;

	[Header("On Start")]
	public S_O_StageScenes _sceneToGoToOnStart;
	public int          _framesBeforeLoading = 30;
	public AudioSource      _AudioOnStart;

	[Header("Fading")]
	public Image    _BlackFade;
	public int      _framesToFadeInOnLoad = 30;
	public int      _framesToFadeOutOnStart = 20;

	//For tracking fade
	private float   _currentAlpha = 1;

	void Start () {
		_BlackFade.enabled = true;

		StartCoroutine(FadeBlack(_BlackFade, 0, _framesToFadeInOnLoad, _currentAlpha));
		_currentAlpha = 0;
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_Measurers, "MoveIn", _delayBeforeMeasurersEnter));
	}

	//Lerps from current alpha of the fade image to set alpha smoothly over desired frames
	public static IEnumerator FadeBlack ( Image Fade, int goalAlpha, float frames, float currentAlpha ) {

		float startAlpha = currentAlpha;
		for (float i = 1 ; i < frames + 1 ; i++)
		{
			yield return new WaitForFixedUpdate();
			currentAlpha = Mathf.Lerp(startAlpha, goalAlpha, i / frames);
			ApplyColour(Fade, currentAlpha);
		}
	}

	private static void ApplyColour ( Image Fade, float currentAlpha ) {
		Color a = Color.black;
		a.a *= currentAlpha;
		Fade.color = a;
	}

	//Called by a start button and starts the animation, then goes onto the inputting scene.
	public void PressStart () {
		S_CarryAcrossScenes.whatIsCurrentSceneType = S_CarryAcrossScenes.EnumGameSceneTypes.Menus;
		StartCoroutine(DelayMovingToNextScene(_sceneToGoToOnStart, _framesBeforeLoading, S_CarryAcrossScenes.EnumGameSceneTypes.Menus));
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_Measurers, "MoveOut", _delayBeforeMeasurersEnter));
		if (_AudioOnStart != null) _AudioOnStart.Play();
	}

	public static IEnumerator DelayMovingToNextScene ( S_O_StageScenes Scene, int frames, S_CarryAcrossScenes.EnumGameSceneTypes NewSceneType, Action OnLoad = null ) {
		//Allow any animation to finish.
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		bool hasSubScenes = Scene._Scenes != null && Scene._Scenes.Length > 0;

		//Tracking progress of loading scenes overall
		float overallProgress = 0;
		float mainProgress = 0;

		//Tracking time taken
		float minSecondsToLoadScene = Scene._minSecondsToLoadScenes;
		float secondsTaken = 0;

		AsyncOperation mainScene = SceneManager.LoadSceneAsync(Scene._BaseScene);
		mainScene.allowSceneActivation = false;

		while (mainProgress < 0.9f || (secondsTaken <= minSecondsToLoadScene && !hasSubScenes))
		{
			UpdateEachFrame();
			yield return new WaitForEndOfFrame();
		}

		//If no more scenes, finish loading level here.
		if (!hasSubScenes)
		{
			mainScene.allowSceneActivation = true;
			OnLoadComplete();
			yield break;
		}

		//If subscenes, they need to be loaded before control is given to the player.

		//How many scenes the stage has.
		List<AsyncOperation> loadingAllScenes = new List<AsyncOperation>();
		int sceneCount = 0;
		overallProgress = 0;

		//If there are any sub-scenes, start to load them as well.
		foreach (SceneField subScene in Scene._Scenes)
		{
			//Add subscene and ensure can't activate until told.
			loadingAllScenes.Add(SceneManager.LoadSceneAsync(subScene, LoadSceneMode.Additive));
			loadingAllScenes[sceneCount].allowSceneActivation = false;
			sceneCount++;
			Debug.Log("Async loading " + subScene);
		}

		mainScene.allowSceneActivation = true;


		//Wait until all scenes are loaded and ready to activate simultaniously. 
		while (overallProgress < .8999999f || secondsTaken <= minSecondsToLoadScene)
		{
			yield return new WaitForEndOfFrame();
			UpdateEachFrame();

			//Get average progress of each scene.
			float currentProgress = 0;
			for (int i = 0 ; i < loadingAllScenes.Count ; i++)
			{
				currentProgress += loadingAllScenes[i].progress;
				Debug.Log(loadingAllScenes[i].progress);
			}
			overallProgress = currentProgress / (sceneCount);
		}

		for (int i = 0 ; i < sceneCount ; i++)
			loadingAllScenes[i].allowSceneActivation = true;

		OnLoadComplete();

		yield break;
		void UpdateEachFrame () {

			mainProgress = mainScene.progress;
			secondsTaken += Time.deltaTime;

			if (!hasSubScenes)
				overallProgress = mainProgress;
		}

		void OnLoadComplete () {
			//Decides what objects can be carried to over to the next scene.
			S_CarryAcrossScenes.whatIsCurrentSceneType = NewSceneType;

			if (OnLoad != null)
				OnLoad.Invoke();
		}
	}

	//Called by an exit button and immediately ends the game.
	public void PressQuit () {
		Application.Quit();
	}
}
