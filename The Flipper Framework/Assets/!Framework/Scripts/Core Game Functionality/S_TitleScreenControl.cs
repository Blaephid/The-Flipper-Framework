using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_TitleScreenControl : MonoBehaviour
{

	[Header("Animations")]
	public Animator     _Measurers;
	public int          _delayBeforeMeasurersEnter;

	[Header("On Start")]
	public string	_sceneToGoToOnStart = "Sc_CharacterSelect";
	public int          _framesBeforeLoading = 30;
	public AudioSource	_AudioOnStart;

	[Header("Fading")]
	public Image	_BlackFade;
	public int	_framesToFadeInOnLoad = 30;
	public int	_framesToFadeOutOnStart = 20;

	//For tracking fade
	private float	_currentAlpha = 1;

	void Start () {
		_BlackFade.enabled = true;

		StartCoroutine(FadeBlack(_BlackFade,0, _framesToFadeInOnLoad, _currentAlpha));
		_currentAlpha = 0;
		StartCoroutine(TriggerAnimatorAfterDelay(_Measurers,"MoveIn", _delayBeforeMeasurersEnter));
	}

	//Lerps from current alpha of the fade image to set alpha smoothly over desired frames
	public static IEnumerator FadeBlack (Image Fade ,int goalAlpha, float frames, float currentAlpha ) {

		float startAlpha = currentAlpha;
		for (float i = 1 ; i < frames + 1 ; i++)
		{
			yield return new WaitForFixedUpdate();
			currentAlpha = Mathf.Lerp(startAlpha, goalAlpha, i / frames);
			ApplyColour(Fade, currentAlpha);
		}
	}

	private static void ApplyColour (Image Fade, float currentAlpha ) {
		Color a = Color.black;
		a.a *= currentAlpha;
		Fade.color = a;
	}

	public static IEnumerator TriggerAnimatorAfterDelay (Animator Measurers, string trigger, int frames) {
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		Measurers.SetTrigger(trigger);
	}

		//Called by a start button and starts the animation, then goes onto the inputting scene.
	public void PressStart () {
		S_CarryAcrossScenes.whatIsCurrentSceneType = S_CarryAcrossScenes.EnumGameSceneTypes.Menus;
		StartCoroutine(DelayMovingToNextScene(_sceneToGoToOnStart, _framesBeforeLoading));
		StartCoroutine(TriggerAnimatorAfterDelay(_Measurers, "MoveOut", _delayBeforeMeasurersEnter));
		_AudioOnStart.Play();
	}

	public static IEnumerator DelayMovingToNextScene (string scene, int frames ) {
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		SceneManager.LoadScene(scene);
	}

		//Called by an exit button and immediately ends the game.
		public void PressQuit () {
		Application.Quit();
	}
}
