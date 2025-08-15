using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;
using TMPro;

public class S_SelectMenu : MonoBehaviour
{

	[Header("Animations")]
	public Animator     _Measurers;
	public int          _delayBeforeMeasurersEnter;
	public Animation[]  _AdditionalAnimations;

	[Header("Stage Screen")]
	public GameObject _StageScreenObject;
	public Animator _StageScreenAnimator;
	public TextMeshProUGUI _StageNameText;
	public TextMeshProUGUI _CharacterText;
	public int          _framesBeforeStageScreen = 20;

	[Header("Loading Next Scene")]
	public GameObject   _GoButton;
	public int          _framesBeforeLoading;

	[Header("Current")]
	public GameObject _SelectedCharacter;
	public S_O_StageScenes _SelectedStageObject;
	private bool _hasSelectedStage = false;

	// Start is called before the first frame update
	void Start () {
		_SelectedCharacter = null;
		_SelectedStageObject = null;
		//_selectedStage = null;
		_GoButton.SetActive(false);

		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_Measurers, "MoveIn", _delayBeforeMeasurersEnter));
		_StageScreenObject.SetActive(false);
	}

	private void CheckIfSelected () {
		if (_SelectedCharacter && _hasSelectedStage  && _GoButton)
		{
			_GoButton.SetActive(true);
		}
		else
			_GoButton.SetActive(false);
	}


	public void AssignStageObject ( S_O_StageScenes LevelSceneObject ) {
		_SelectedStageObject = LevelSceneObject;
		_hasSelectedStage = true;
		if(_StageNameText)
			_StageNameText.text = LevelSceneObject._StageName;
		CheckIfSelected();
	}

	public void AssignCharacter ( S_O_CharactersForMenu Character ) {
		_SelectedCharacter = Character._Prefab;
		if (_CharacterText)
			_CharacterText.text = Character._displayName;
		CheckIfSelected();
	}


	//Activated by the go button in the level.
	public void StartLevel () {

		GameObject[] MusicObject = GameObject.FindGameObjectsWithTag("Music");
		if (MusicObject != null && MusicObject.Length > 0)
			if (MusicObject[0].TryGetComponent(out AudioSource Source))
				StartCoroutine(S_S_Objects.LerpAudioSourceVolume(Source, 1, 0));

		//Exit animation
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_Measurers, "MoveOut", _delayBeforeMeasurersEnter));
		foreach(Animation anim in _AdditionalAnimations)
			StartCoroutine(S_S_Objects.TriggerAnimationAfterDelay(anim, 2));

		//Start loading
		StartCoroutine(S_TitleScreenControl.DelayMovingToNextScene(_SelectedStageObject, _framesBeforeLoading, S_CarryAcrossScenes.EnumGameSceneTypes.Overworld, OnLoad));
		_StageScreenObject.SetActive(true);
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_StageScreenAnimator, "Enter", _framesBeforeStageScreen));
	}

	public void OnLoad () {
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_StageScreenAnimator, "Exit", 8));
	}
}

