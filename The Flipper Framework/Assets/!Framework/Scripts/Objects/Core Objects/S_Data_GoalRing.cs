using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Data_GoalRing : S_Data_Base
{
	public AudioSource _OnTouchAudio;
	public AudioSource _ConstantAudio;

	[ColourIfNull(1,0,0,1)]public S_StageCompleteControl _StageEndController;

	public override void OnGet ( Transform Player ) {
		base.OnGet(Player);

		_OnTouchAudio.Play();
	}

	public void OnStageEnd (S_PlayerScore Score) {
		_StageEndController.gameObject.SetActive(true);
		gameObject.SetActive(true);
		_StageEndController.OnStageEnd(Score);
	}
}
