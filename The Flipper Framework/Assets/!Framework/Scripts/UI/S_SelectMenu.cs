using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class S_SelectMenu : MonoBehaviour
{

	[Header("Animations")]
	public Animator     _Measurers;
	public int          _delayBeforeMeasurersEnter;

	[Header("Fading")]
	public Image	_BlackFade;
	public int          _framesToFadeOutOnStart;

	[Header("Loading Next Scene")]
	public GameObject   _GoButton;
	public int          _framesBeforeLoading;

	[Header("Current")]
	public GameObject _SelectedCharacter;
	public string       _selectedStage;

	private float _currentAlpha = 0;

	// Start is called before the first frame update
	void Start () {
		_SelectedCharacter = null;
		_selectedStage = null;
		_GoButton.SetActive(false);

		StartCoroutine(S_TitleScreenControl.TriggerAnimatorAfterDelay(_Measurers, "MoveIn", _delayBeforeMeasurersEnter));
	}

	private void CheckIfSelected () {
		if (_SelectedCharacter && _selectedStage != null && _GoButton)
		{
			_GoButton.SetActive (true);
		}
	}

		public void AssignStage (string stageName) {
		_selectedStage=stageName;
		CheckIfSelected();
	}

	public void AssignCharacter (GameObject Character) {
		_SelectedCharacter=Character;
		CheckIfSelected ();
	}

	public void StartLevel () {
		StartCoroutine(S_TitleScreenControl.FadeBlack(_BlackFade, 1, _framesToFadeOutOnStart, _currentAlpha));
		_currentAlpha = 1;
		StartCoroutine(S_TitleScreenControl.TriggerAnimatorAfterDelay(_Measurers, "MoveOut", _delayBeforeMeasurersEnter));
		S_CarryAcrossScenes.whatIsCurrentSceneType = S_CarryAcrossScenes.EnumGameSceneTypes.Overworld;
		StartCoroutine(S_TitleScreenControl.DelayMovingToNextScene(_selectedStage, _framesBeforeLoading));
	}
}
