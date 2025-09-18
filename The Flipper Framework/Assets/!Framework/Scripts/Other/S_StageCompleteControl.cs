using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class S_StageCompleteControl : MonoBehaviour
{
	[Header("External")]
	[ColourIfNull(1,0,0,1)]public S_SpawnCharacter _Spawner;
	//[ColourIfNull(1,0,0,1)]public SceneField _StageCompleteScene;
	[Header("Internal")]
	[ColourIfNull(1,0,0,1)]public Animator _Animator;
	[ColourIfNull(1,0,0,1)]public GameObject _Camera;
	[ColourIfNull(1,0,0,1)]public GameObject _PostProcessing;

	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _StageName;

	[Header("Rank Texts")]
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _RingRankText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _TimeRankText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _TotalRankText;

	[Header("Score Texts")]
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _MinutesText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _SecondsText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _MilisecondsText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _RingsGainedText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _RingsLostText;

	[Header("Next Rank Texts")]
	[ColourIfNull(1,0,0,1)]public GameObject _TimeNextRank;
	[ColourIfNull(1,0,0,1)]public GameObject _RingsNextRank;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _TimeNextRankNote;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _RingsNextRankNote;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _MinutesNextText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _SecondsNextText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _MilisecondsNextText;
	[ColourIfNull(1,0,0,1)]public TextMeshProUGUI _RingsNextText;

	[Header("Audio")]
	[ColourIfNull(1,0,0,1)]public AudioSource _ResultsMusic;
	[ColourIfNull(1,0,0,1)]public AudioSource _PostResultsMusic;
	[ColourIfNull(1,0,0,1)]public AudioSource _MinorRankSound;
	[ColourIfNull(1,0,0,1)]public AudioSource _MajorRankSound;

	private bool _canContinue = false;

	public void OnStageEnd ( S_PlayerScore Score ) {

		_Spawner.OnStageEnd(); //To disable post processing and replace

		_Camera.SetActive(true);
		_PostProcessing.SetActive(true);

		_ResultsMusic.Play();

		_Animator.SetTrigger("Enter");
		_StageName.text = Score._StageInfo._StageName;

		Score.CalculateRank();

		//Display
		_RingRankText.text = Score._ringsRankText;
		_TimeRankText.text = Score._timeRankText;
		_TotalRankText.text = Score._totalRankText;

		DisplayNextRank(Score);

		StartCoroutine(BuildUpScoreDisplays(Score));
	}

	private void DisplayNextRank ( S_PlayerScore Score ) {
		if (Score._toNextRankTime == Vector3.zero) { _TimeNextRank.SetActive(false); }
		else
		{
			switch (Score._timeRankText)
			{
				case "S":
					_TimeNextRankNote.text = "Secret Rank At";
					break;
				case "A":
					_TimeNextRankNote.text = "Max Rank At";
					break;
				default:
					_TimeNextRankNote.text = "Next Rank At";
					break;
			}

			_MinutesNextText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._toNextRankTime.x);
			_SecondsNextText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._toNextRankTime.y);
			_MilisecondsNextText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._toNextRankTime.z);
		}
		if (Score._toNextRankRings == 0) { _RingsNextRank.SetActive(false); }
		else
		{
			switch (Score._ringsRankText)
			{
				case "A":
					_RingsNextRankNote.text = "Max Rank At";
					break;
				default:
					_RingsNextRankNote.text = "Next Rank At";
					break;
			}
			_RingsNextText.text = Score._toNextRankRings.ToString();
		}
	}

	//Gives the value a "calculating result" effect, before revealing ranks.
	private IEnumerator BuildUpScoreDisplays ( S_PlayerScore Score ) {
		float count = 0;

		float[] secondsForLockInOfElement = new float[] {2.5f, 3f, 3.5f, 4f, 4.5f, 5.5f };

		while (count <= secondsForLockInOfElement[5])
		{
			yield return new WaitForEndOfFrame();
			count += Time.deltaTime;


			//For minutes, seconds, milliseconds, rings gained, and ring lost, give random numbers quickly until locking them in.

			if (count < secondsForLockInOfElement[0])
				_MinutesText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits(Random.Range(0, 99));
			else
				_MinutesText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._minutes);

			if (count < secondsForLockInOfElement[1])
				_SecondsText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits(Random.Range(0, 59));
			else
				_SecondsText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._seconds);

			if (count < secondsForLockInOfElement[2])
				_MilisecondsText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits(Random.Range(0, 99));
			else
				_MilisecondsText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._milliseconds);

			if (count < secondsForLockInOfElement[3])
				_RingsGainedText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits(Random.Range(0, 999));
			else
				_RingsGainedText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._ringScoreGained, 3);

			if (count < secondsForLockInOfElement[4])
				_RingsLostText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits(Random.Range(0, 999));
			else
				_RingsLostText.text = S_S_MoreMaths.DisplayIntAsStringInXDigits((int)Score._ringScoreLost, 3);

		}

		_Animator.SetTrigger("Calculated");

		yield return new WaitForSeconds(1.5f);

		_canContinue = true;
	}

	public void AnimPostResultsMusic () {
		_PostResultsMusic.Play();
	}

	public void AnimMinorSound () {
		_MinorRankSound.Play();
	}

	public void AnimMajorSound () {
		_MajorRankSound.Play();
	}

	//Because the control map is still attached to the character, this is searched for and called by the S_PlayerInput Script. See the player prefab.
	public void InputContinue ( InputAction.CallbackContext ctx ) {

		if (ctx.performed && _canContinue)
		{
			S_Manager_LevelProgress.ReturnToTitleScreen();
			//SceneManager.LoadScene(_StageCompleteScene);
		}
	}

}
