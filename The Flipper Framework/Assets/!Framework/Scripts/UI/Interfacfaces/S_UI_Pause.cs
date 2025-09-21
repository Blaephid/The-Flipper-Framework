using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_UI_Pause : MonoBehaviour
{
	public S_UI_Options _Options;
	public GameObject _Pause;
	public Animator   _PauseAnimator;

	private bool _isPausedLocal;
	void Start () {

		SetPanelActive(false);
	}

	
	public void AnEventClosed () {
		SetPanelActive(false);
	}

	public void AnEventOpened () {
		SetPanelActive(true);
	}

	public void SetPanelActive(bool set ) {
		_Pause.SetActive(set);
	}

	public void PauseToggle () {

		//ENTER
		if (_isPausedLocal)
		{

			_PauseAnimator.SetTrigger("Exit");
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			GameObject.FindFirstObjectByType<S_ActionManager>()._isPaused = false;
			_isPausedLocal = false;

			if (!_Options._isOptionsOpen)
				StartCoroutine(WaitBeforeSettingTimeScale(1, 0.05f));
			else
				StartCoroutine(WaitBeforeSettingTimeScale(1, 0.45f));
		}
		//EXIT
		else
		{
			SetPanelActive(true);
			_PauseAnimator.SetTrigger("Enter");
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			GameObject.FindFirstObjectByType<S_ActionManager>()._isPaused = true;
			_isPausedLocal = true;

			_Options.OnPauseOpen();

			StartCoroutine(WaitBeforeSettingTimeScale(0, 0.05f));
		}

	}

	//Gives one frame for any _isPaused effects to happen.
	private IEnumerator WaitBeforeSettingTimeScale (float set, float seconds) {
		yield return new WaitForSecondsRealtime(seconds);
		Time.timeScale = set;
	}

	public void Resume () {
		PauseToggle();
	}
	public void Quit () {
		PauseToggle();

		S_Manager_LevelProgress.ReturnToTitleScreen();
	}

	public void FindRestartLevelAndRestart() {
		S_SelectMenu StageManagement = GameObject.FindFirstObjectByType<S_SelectMenu>(FindObjectsInactive.Include);

		if (StageManagement)
		{
			S_SpawnCharacter Spawner = GameObject.FindFirstObjectByType<S_SpawnCharacter>(FindObjectsInactive.Include);
			StartCoroutine(S_S_Objects.LerpAudioSourceVolume(Spawner._MusicPlayer, 0.8f, 0f));

			StageManagement.AssignObjectIfSpawnerIsPresent();
			StageManagement.StartLevel();
		}
	}
}
