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
	public StrucTriggerAnimation[]  _AnimationsOnGo;

	[Header("Stage Screen")]
	public GameObject _StageScreenObject;
	public Animator _StageScreenAnimator;
	public TextMeshProUGUI _StageNameText;
	public TextMeshProUGUI _CharacterText;
	public float          _secondsBeforeStageScreen = 20;

	[Header("Loading Next Scene")]
	public GameObject   _GoButton;
	public float          _secondsBeforeLoading;

	[Header("Current")]
	[NonSerialized]
	public S_O_CharactersForMenu _SelectedCharacter;
	[NonSerialized]
	public S_O_StageScenes _SelectedStageObject;
	private bool _hasSelectedStage = false;

	private bool _isLoadingStage = false;

	// Start is called before the first frame update
	void Awake () {
		
		if(_GoButton)
			_GoButton.SetActive(false);

		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_Measurers, "MoveIn", _delayBeforeMeasurersEnter));
		_StageScreenObject.SetActive(false);

	}

	//For instances of this menu present in the stage scenes or spawned by the player.
	//Due to "dontdestroyonload", this will only be present when loading scenes directly in editor, as the one from the stage select screen will take priority.
	public void AssignObjectIfSpawnerIsPresent () {
		S_SpawnCharacter Spawner = GameObject.FindFirstObjectByType<S_SpawnCharacter>();
		if(!Spawner) { return; }

		AssignStageObject(Spawner._StageInfo);
		AssignCharacter(Spawner._CharacterToSpawn);
	}

	private void CheckIfSelected () {
		if(!_GoButton) { return; }
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
		_SelectedCharacter = Character;
		if (_CharacterText)
			_CharacterText.text = Character._displayName;
		CheckIfSelected();
	}


	//Activated by the go button in the level.
	public void StartLevel () {
		if(_isLoadingStage || !_SelectedStageObject || !_SelectedCharacter ) { return; }

		gameObject.SetActive(true);
		GameObject[] MusicObject = GameObject.FindGameObjectsWithTag("Music");
		if (MusicObject != null && MusicObject.Length > 0)
			if (MusicObject[0].TryGetComponent(out AudioSource Source))
				StartCoroutine(S_S_Objects.LerpAudioSourceVolume(Source, 1, 0));

		//Exit animations
		foreach(StrucTriggerAnimation anim in _AnimationsOnGo)
		{
			StartCoroutine(S_S_Objects.TriggerAnimationAfterDelay(anim.AnimClip, 2));
			StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(anim.Animator, anim.trigger, 0, 0.05f));
		}
		//Start loading
		_isLoadingStage = true;
		_StageScreenObject.SetActive(true);
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_StageScreenAnimator, "Enter", 0, _secondsBeforeStageScreen));
		StartCoroutine(S_TitleScreenControl.DelayMovingToNextScene(_SelectedStageObject, _secondsBeforeLoading, S_CarryAcrossScenes.EnumGameSceneTypes.Overworld, OnLoad));
	}

	private void OnDestroy () {
		Debug.Log(gameObject + "Was destoyed");
	}

	public void OnLoad () {
		_isLoadingStage = false;
		Time.timeScale = 1;
		StartCoroutine(S_S_Objects.TriggerAnimatorAfterDelay(_StageScreenAnimator, "Exit", 8));
	}
}

