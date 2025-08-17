using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_UI_Pause : MonoBehaviour
{
	public GameObject _Pause;
	public Animator   _PauseAnimator;
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

		if (_Pause.activeSelf)
		{
			_PauseAnimator.SetTrigger("Exit");
			Time.timeScale = 1;
		}
		else
		{
			SetPanelActive(true);
			_PauseAnimator.SetTrigger("Enter");
			Time.timeScale = 0;
		}

	}

	public void Resume () {
		PauseToggle();
	}
	public void Quit () {
		PauseToggle();

		S_Manager_LevelProgress.ReturnToTitleScreen();
	}

}
