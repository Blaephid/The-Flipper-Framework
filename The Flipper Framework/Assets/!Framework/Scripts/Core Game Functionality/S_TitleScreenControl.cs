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

	public static IEnumerator DelayMovingToNextScene ( S_O_StageScenes Scene, float seconds, S_CarryAcrossScenes.EnumGameSceneTypes NewSceneType, Action OnLoad = null ) {

		if (!S_SpawnCharacter.s_CanSpawn) { yield break; } //Cant have multiple running at once
		S_SpawnCharacter.s_CanSpawn = false; //Ensures player will not spawn until level is COMPLETELY loaded, including important scenes.

		//Allow any animation to finish.
		yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, seconds)); //Use seconds rather than frames because this can be called when timescale is 0
		
		bool hasSubScenes = Scene._Scenes != null && Scene._Scenes.Length > 0;

		//Tracking progress of loading scenes overall
		float overallProgress = 0;
		float mainProgress = 0;

		//Tracking time taken
		float minSecondsToLoadScene = Scene._minSecondsToLoadScenes;
		float secondsTaken = 0;

		Time.timeScale = 0.01f;

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
			OnReadyComplete();
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

		bool allLevelsReady = false;

		//Wait until all scenes are mostly loaded and ready to activate simultaniously. 
		while (!allLevelsReady || secondsTaken <= minSecondsToLoadScene)
		{
			yield return new WaitForEndOfFrame();
			UpdateEachFrame();

			bool allReady = true;
			for (int i = 0 ; i < loadingAllScenes.Count ; i++)
			{
				Debug.Log(loadingAllScenes[i].progress);
				if (loadingAllScenes[i].progress < 0.9f)
				{
					allReady = false;
					break;
				}
			}

			allLevelsReady = allReady;
		}

		for (int i = 0 ; i < sceneCount ; i++)
			loadingAllScenes[i].allowSceneActivation = true;

		OnReadyComplete();

		bool allLevelsLoaded = false;

		//When all scenes have activate, spawn character.
		while (!allLevelsLoaded)
		{
			yield return new WaitForEndOfFrame();

			bool allLoaded = true;
			for (int i = 0 ; i < loadingAllScenes.Count ; i++)
			{
				Debug.Log(loadingAllScenes[i].isDone);
				if (!loadingAllScenes[i].isDone)
				{
					allLoaded = false;
					break;
				}
			}
			allLevelsLoaded = allLoaded;
		}

		OnLoadComplete();

		yield break;
		void UpdateEachFrame () {

			mainProgress = mainScene.progress;
			secondsTaken += Time.unscaledDeltaTime;

			if (!hasSubScenes)
				overallProgress = mainProgress;
		}
		void OnReadyComplete () {
			//Decides what objects can be carried to over to the next scene.
			//For instance, menu unique objects should not be in a stage. Called seperately from load complete as whether or not to destroy these objects is based on the sceneloaded event.
			S_CarryAcrossScenes.whatIsCurrentSceneType = NewSceneType;
		}
		void OnLoadComplete () {
			//Once all scenes are ready, start the game
			Time.timeScale = 1f;
			S_SpawnCharacter.s_CanSpawn = true;

			if (OnLoad != null)
				OnLoad.Invoke();
		}
	}

	//Called by an exit button and immediately ends the game.
	public void PressQuit () {
		Application.Quit();
	}
}
