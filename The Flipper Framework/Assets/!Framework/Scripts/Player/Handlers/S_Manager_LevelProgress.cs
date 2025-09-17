using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEditor;

public class S_Manager_LevelProgress : S_Player_Base
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	public static event EventHandler OnReset;
	public static event EventHandler OnDeath;
	public static event EventHandler OnStageClear;

	private S_Control_EffectsPlayer _Effects;
	private S_Handler_HealthAndHurt _HealthAndHurt;
	private S_PlayerScore _Score;

	[Header("On Level End")]
	public SceneField               _StageCompleteScene;
	#endregion

	// Trackers
	#region trackers
	[NonSerialized]
	public S_SpawnCharacter _Spawner;

	//Reset transforms. Set on start and by checkpoints.
	public Transform _respawnTransform { get; set; }
	public Vector3 _respawnPosition { get; set; }
	public Quaternion _respawnRotation { get; set; }
	private Vector3         _respawnForwards;
	public LaunchPlayerData _RespawnLaunch { get; set; }
	public Animator _RespawnAnimator;

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	public override void Awake () {
		base.Awake();

		_HealthAndHurt = _Tools.GetComponent<S_Handler_HealthAndHurt>();
		_Effects = _Tools.EffectsControl;
		_Score = _Tools.GetComponent<S_PlayerScore>();

		_MainSkin = _Tools.MainSkin;

		SetCheckPoint(_Spawner.transform, _Spawner);

		//_CamHandler._HedgeCam.SetBehind(20); //Sets camera back to behind player.
	}


	//Since certain objects relate to progressing through on ending a level, they are handled here.
	public void EventTriggerEnter ( Collider Col ) {
		switch (Col.tag)
		{
			//For saving new respawn points.
			case "Checkpoint":
				if (Col.TryGetComponent(out S_Data_Checkpoint CheckPointScript))
				{
					if (!CheckPointScript._IsOn)
					{
						//Effects on object
						CheckPointScript._IsOn = true;
						Col.GetComponent<AudioSource>().Play();
						foreach (Animator anim in CheckPointScript.Animators)
						{
							anim.SetTrigger("Open");
						}
						CheckPointScript.Laser.SetActive(false);

						//Local data
						SetCheckPoint(CheckPointScript.CheckPos);
					}

				}
				break;

			case "Special":
				if (Col.TryGetComponent(out S_Data_GoalRing GoalRingData))
				{
					GoalRingData.OnGet(transform);

					StartCoroutine(TransitionToStageComplete(GoalRingData));
				}
				break;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private IEnumerator TransitionToStageComplete ( S_Data_GoalRing GoalRingData, float totalTime = 2.5f, float timeToSlow = 1.5f ) {

		_Score._paused = true;

		//Disables on control of character.
		_Input._move = Vector3.zero;
		_Input._completeControlLock = true;
		S_S_Logic.AddLockToList(ref _PlayerPhys._locksForCanControl, "NextStage");

		_Input._completeControlLock = true;

		yield return new WaitForSeconds(0.2f);

		StartCoroutine(S_S_Objects.LerpAudioSourceVolume(_CoreValues._Music, 2, 0));

		if (OnStageClear != null)
			OnStageClear.Invoke(this, EventArgs.Empty);

		float timeCount = 0;
		while (timeCount <= totalTime && totalTime > 0)
		{
			yield return new WaitForEndOfFrame();

			timeCount += Time.unscaledDeltaTime;

			Time.timeScale = Mathf.Lerp(1, 0.03f, timeCount / timeToSlow);
			_HealthAndHurt._FadeOutImage.color = Color.Lerp(_HealthAndHurt._FadeOutImage.color, Color.black, timeCount / totalTime);
		}
		Time.timeScale = 1;

		_Tools.GetComponent<PlayerInput>().SwitchCurrentActionMap("Stage Complete");

		//Activates the stage complete screen.
		GoalRingData.OnStageEnd(_Score);

		//Effects on character
		_CoreUIElements._HudRoot.SetActive(false);
		_MainSkin.gameObject.SetActive(false);
		_Actions._isPaused = true;

		_PlayerPhys._arePhysicsEnabled = false;
		_PlayerVel.SetBothVelocities(Vector3.zero, Vector2.one);
		_PlayerVel.SetTotalVelocity(); //Because fixedupdates will be called less, and physics are disabled
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Called as soon as the fade to black is completed, and calls an event that should reset the level to how it was at the start (the player is handled in other methods). Remember that these events will be set locally in their own scripts.
	public void CallRespawnEvents () {
		if (OnReset != null)
		{
			//EditorApplication.isPaused = true;
			Debug.Log(OnReset.GetInvocationList().Length);
			if (OnReset != null)
			{
				foreach (var d in OnReset.GetInvocationList())
				{
					UnityEngine.Object target = d.Target as UnityEngine.Object;
					Debug.Log($"Listener: {d.Method.Name} on {target}");
				}
			}

			OnReset.Invoke(this, EventArgs.Empty);
		}
		if (OnDeath != null)
		{
			OnDeath.Invoke(this, EventArgs.Empty);
		}

	}

	//Called after enough time has passed after death, right before removing the fade to black. This will reposition the player, but trackers (like physic checkers) are reset in the Handler_HealthAndHurt script.
	public void ResetToCheckPoint () {

		//Temporarily prevents movement of any kind.
		_Input.LockInputForAWhile(9, true, Vector3.zero);
		_Actions.LockAirMovesForFrames(9);

		//Ends hurt state.
		_Actions._ActionDefault.StartAction();

		//Ensure efffects are disabled.
		_Effects.EnableLargeTrail(0);

		//In case was killed by something that bypassed shield.
		_HealthAndHurt.SetShield(false);

		//Transform
		_PlayerPhys.SetPlayerPosition(_respawnPosition);
		_PlayerPhys.SetPlayerRotation(Quaternion.identity.normalized, true);
		_MainSkin.forward = _respawnForwards;

		//Ensures rotation is correct and can lead into instant movement.
		_PlayerVel.SetBothVelocities(_MainSkin.forward * 0.05f, new Vector2(1, 0));

		//Camera
		_CamHandler._HedgeCam._lookTimer = 0;
		_CamHandler._HedgeCam.SetBehind(0); //Sets camera back to behind player.

		//Ensure no fallback is active
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraFallBack(Vector2.zero, 0, 0, 0, 0, "Death"));
		_Tools.MainCamera.transform.position = _CamHandler._HedgeCam.transform.position;
		_Tools.MainCamera.transform.rotation = _CamHandler._HedgeCam.transform.rotation;
	}

	public void TriggerAnimatorOnRespawn () {
		if (!_RespawnAnimator) { return; }

		_RespawnAnimator.enabled = true;
		_RespawnAnimator.SetTrigger("Start");
	}

	public void LaunchOnRespawn () {
		LaunchFromCheckpoint(true, _RespawnLaunch, _respawnTransform);
	}

	public void LaunchFromCheckpoint ( bool launch, LaunchPlayerData launchData, Transform transform ) {

		if (!launch || !_Actions._ObjectForInteractions.TryGetComponent(out S_Interaction_Objects Objects)) { return; }

		//Applying launch
		if (launchData._force_ <= 0 && launchData._directionToUse_.sqrMagnitude <= 1) { return; }

		_RespawnLaunch = launchData;
		_respawnTransform = transform;

		Objects.LaunchInDirection(launchData._directionToUse_, launchData._force_, transform, Objects.transform, launchData);

	}

	//Checkpoints simply retain transform data, as the level will always reset to its base.
	public void SetCheckPoint ( Transform checkPointTransform, S_SpawnCharacter SpawnerAtStartOfLevel = null, Animator RespawnAnimator = null ) {
		_Spawner = SpawnerAtStartOfLevel;
		_respawnTransform = checkPointTransform;
		_respawnPosition = checkPointTransform.position;
		_respawnForwards = checkPointTransform.forward;
		_RespawnLaunch = _Spawner && _Spawner._launch ? _Spawner._launchOnSpawnData_ : new LaunchPlayerData();
		_RespawnAnimator = _Spawner ? _Spawner._AnimatorOnSpawn : RespawnAnimator;

		_CoreValues.SaveValuesOnCheckpoint();
	}
	#endregion

	//Temporary Debug Command to reset. Called by an input set in editor.
	public void ReturnToTitleScreenImmediately () {
		ReturnToTitleScreen();
	}

	public static void ReturnToTitleScreen () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		S_CarryAcrossScenes.whatIsCurrentSceneType = S_CarryAcrossScenes.EnumGameSceneTypes.Menus;
		SceneManager.LoadScene(0);
	}
}
