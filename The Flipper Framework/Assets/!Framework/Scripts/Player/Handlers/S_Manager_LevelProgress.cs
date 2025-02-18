using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class S_Manager_LevelProgress : MonoBehaviour
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	public static event EventHandler OnReset;


	private S_CharacterTools		_Tools;
	private S_Handler_HealthAndHurt	_HealthAndHurt;

	private S_ActionManager		_Actions;
	private S_PlayerPhysics	_PlayerPhys;
	private S_PlayerVelocity	_PlayerVel;
	private S_Handler_Camera	_CamHandler;
	private S_PlayerInput	_Input;

	private Transform		_MainSkin;

	public AudioClip		_GoalRingTouchingSound;

	private Collider              _GoalRingObject;
	#endregion

	// Trackers
	#region trackers
	//Reset transforms. Set on start and by checkpoints.
	public Vector3		_resumePosition { get; set; }
	public Quaternion		_resumeRotation { get; set; }
	private Vector3		_resumeForwards;

	//
	public string		_nextLevelNameLeft;
	public string		_nextLevelNameRight;

	//Tracking ending levels.
	private bool		_readyForNextStage = false;
	private float		_readyCount = 0;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Awake () {
		

		_Tools =		GetComponentInParent<S_CharacterTools>();
		_CamHandler =	_Tools.CamHandler;
		_Actions =	_Tools._ActionManager;
		_PlayerPhys =	_Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel =	_Tools.GetComponent<S_PlayerVelocity>();
		_Input =		_Tools.GetComponent<S_PlayerInput>();
		_HealthAndHurt =	_Tools.GetComponent<S_Handler_HealthAndHurt>();

		_MainSkin =	_Tools.MainSkin;

		_resumePosition =	_MainSkin.position;
		_resumeRotation =	_MainSkin.rotation;
		_resumeForwards =	_MainSkin.forward;

		_CamHandler._HedgeCam.SetBehind(20); //Sets camera back to behind player.
	}

	// Update is called once per frame
	void Update () {
		TransitionToNextStage();
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

			case "GoalRing":
				_readyForNextStage = true; //Sets this to true to the relevant calculations can happen due to the update script in a below method.
				_GoalRingObject = Col;
				break;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private void TransitionToNextStage () {
		if (_readyForNextStage) //Set when touching a goal ring.
		{
			//Disables on control of character.
			_Input._move = Vector3.zero;
			_PlayerPhys._listOfCanControl.Add(false);

			_readyCount += Time.deltaTime;

			//Fade to black.
			if (_readyCount > 0.1f)
			{
				Color alpha = Color.black;
				_HealthAndHurt._FadeOutImage.color = Color.Lerp(_HealthAndHurt._FadeOutImage.color, alpha, Time.fixedTime * 0.1f);
			}

			//Activates the stage complete screen.
			if (_readyCount > 0.6f)
			{
				PlayStageCompleteScene(_GoalRingObject);
			}
		}
	}

	private void PlayStageCompleteScene ( Collider col ) {

		SceneManager.LoadScene("Sc_StageCompleteScreen"); //Switch to scene

		//Play appropriate music.
		col.GetComponent<AudioSource>().clip = _GoalRingTouchingSound;
		col.GetComponent<AudioSource>().loop = false;
		col.GetComponent<AudioSource>().Play();
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Called as soon as the fade to black is completed, and calls an event that should reset the level to how it was at the start (the player is handled in other methods). Remember that these events will be set locally in their own scripts.
	public void RespawnObjects () {
		if (OnReset != null)
		{
			Debug.LogWarning("Has begun respawning");
			OnReset.Invoke(this, EventArgs.Empty);
		}

	}

	//Called after enough time has passed after death, right before removing the fade to black. This will reposition the player, but trackers (like physic checkers) are reset in the Handler_HealthAndHurt script.
	public void ResetToCheckPoint () {

		//Temporarily prevents movement of any kind.
		_Input.LockInputForAWhile(20, true, Vector3.zero);
		StartCoroutine(_Actions.LockAirMovesForFrames(20));

		//Ends hurt state.
		_Actions._ActionDefault.StartAction();

		//Ensure efffects are disabled.
		_Tools.HomingTrailScript.emitTime = 0;
		_Tools.HomingTrailScript.emit = false;

		//In case was killed by something that bypassed shield.
		_HealthAndHurt.SetShield(false);

		//Transform
		_PlayerPhys.SetPlayerPosition(_resumePosition);
		_MainSkin.forward = _resumeForwards;

		//Ensures rotation is correct and can lead into instant movement.
		_PlayerVel.SetBothVelocities(_MainSkin.forward * 2, new Vector2(1, 0));

		//Camera
		_CamHandler._HedgeCam._lookTimer = 0;
		_CamHandler._HedgeCam.SetBehind(20); //Sets camera back to behind player.
	}

	//Checkpoints simply retain transform data, as the level will always reset to its base.
	public void SetCheckPoint ( Transform position ) {
		_resumePosition = position.position;
		_resumeForwards = position.forward;
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
