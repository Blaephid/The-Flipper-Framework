using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class S_Manager_LevelProgress : MonoBehaviour
{

	public static event EventHandler onReset;


	private S_CharacterTools _Tools;
	private S_Handler_HealthAndHurt _HealthAndHurt;

	public Vector3 ResumePosition { get; set; }
	public Quaternion ResumeRotation { get; set; }
	Vector3 ResumeFace;
	public GameObject basePlayer;
	public Transform ResumeTransform;
	S_ActionManager _Actions;
	S_PlayerPhysics _PlayerPhys;
	S_Handler_Camera Cam;
	S_PlayerInput _Input;
	public GameObject CurrentCheckPoint { get; set; }

	Transform characterTransform;
	//public Material LampDone;
	public int LevelToGoNext = 0;
	public string NextLevelNameLeft;
	public string NextLevelNameRight;
	public AudioClip GoalRingTouchingSound;

	bool readyForNextStage = false;
	float readyCount = 0;


	void Start () {
		_Tools = GetComponent<S_CharacterTools>();
		Cam = basePlayer.GetComponent<S_Handler_Camera>();
		_Actions = basePlayer.GetComponent<S_ActionManager>();
		_PlayerPhys = basePlayer.GetComponent<S_PlayerPhysics>();
		_Input = basePlayer.GetComponent<S_PlayerInput>();
		_HealthAndHurt = GetComponent<S_Handler_HealthAndHurt>();
		characterTransform = _Tools.mainSkin;
		ResumePosition = characterTransform.position;
		ResumeRotation = characterTransform.rotation;
		ResumeFace = characterTransform.forward;

	}

	void Update () {
		//LampDone.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);
		//LampDone.SetTextureOffset("_EmissionMap", new Vector2(0, -Time.time) * 3);

		if (readyForNextStage)
		{
			_Input._move = Vector3.zero;
			readyCount += Time.deltaTime;
			if (readyCount > 1.5f)
			{
				_Actions.Action04Control.enabled = false;
				Color alpha = Color.black;
				_Actions.Action04Control._FadeOutImage.color = Color.Lerp(_Actions.Action04Control._FadeOutImage.color, alpha, Time.fixedTime * 0.1f);
			}
			if (readyCount > 2.6f)
			{
				LoadingScreenControl.StageName1 = NextLevelNameLeft;
				LoadingScreenControl.StageName2 = NextLevelNameRight;
				SceneManager.LoadScene(2);
			}
		}
	}


	public void ResetToCheckPoint () {

		_Input.LockInputForAWhile(20, true, Vector3.zero);
		StartCoroutine(_Actions.lockAirMoves(20));
		_Actions.ActionDefault.StartAction();

		_Tools.HomingTrailScript.emitTime = 0;
		_Tools.HomingTrailScript.emit = false;

		if (_HealthAndHurt._hasShield)
		{
			_HealthAndHurt.SetShield(false);
		}


		_PlayerPhys.SetTotalVelocity(characterTransform.forward * 2, new Vector2(1, 0));

		Cam._HedgeCam._isReversed = false;
		Cam._HedgeCam.SetBehind(20);

		transform.position = ResumePosition;
		//transform.rotation = ResumeRotation;
		characterTransform.forward = ResumeFace;

	}

	public void RespawnObjects () {

		if (onReset != null)
		{
			Debug.LogWarning("Has begun respawning");
			onReset.Invoke(this, EventArgs.Empty);
		}

	}

	public void SetCheckPoint ( Transform position ) {
		ResumePosition = position.position;
		ResumeFace = position.forward;
		ResumeTransform = position;
	}

	public void OnTriggerEnter ( Collider col ) {
		if (col.tag == "Checkpoint")
		{
			if (col.GetComponent<S_Data_Checkpoint>() != null)
			{
				//Set Object
				if (!col.GetComponent<S_Data_Checkpoint>().IsOn)
				{

					col.GetComponent<S_Data_Checkpoint>().IsOn = true;
					col.GetComponent<AudioSource>().Play();
					foreach (Animator anim in col.GetComponent<S_Data_Checkpoint>().Animators)
					{
						anim.SetTrigger("Open");
					}
					col.GetComponent<S_Data_Checkpoint>().Laser.SetActive(false);
					SetCheckPoint(col.GetComponent<S_Data_Checkpoint>().CheckPos);
					CurrentCheckPoint = col.gameObject;
					//CurrentCheckPointTimer = GameTimer;
				}

			}
		}
		if (col.tag == "GoalRing")
		{

			readyForNextStage = true;

			StartCoroutine(endLevel(col));

		}
	}

	IEnumerator endLevel ( Collider col ) {
		for (int i = 0 ; i == 2 ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		SceneManager.LoadScene("Sc_StageCompleteScreen");
		//	StageConpleteControl.LevelToGoNext = SceneManager.GetActiveScene ().buildIndex + 1;
		col.GetComponent<AudioSource>().clip = GoalRingTouchingSound;
		col.GetComponent<AudioSource>().loop = false;
		col.GetComponent<AudioSource>().Play();
	}
}
