using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_UI_Pause : MonoBehaviour
{
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

		if (_isPausedLocal)
		{
			_PauseAnimator.SetTrigger("Exit");
			StartCoroutine(WaitOneFrameBeforeSettingTimeScale(1));
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			GameObject.FindFirstObjectByType<S_ActionManager>()._isPaused = false;
			_isPausedLocal = false;
		}
		else
		{
			SetPanelActive(true);
			_PauseAnimator.SetTrigger("Enter");
			StartCoroutine(WaitOneFrameBeforeSettingTimeScale(0));
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			GameObject.FindFirstObjectByType<S_ActionManager>()._isPaused = true;
			_isPausedLocal = true;
		}

	}

	//Gives one frame for any _isPaused effects to happen.
	private IEnumerator WaitOneFrameBeforeSettingTimeScale (float set) {
		yield return new WaitForSecondsRealtime(0.05f);
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
