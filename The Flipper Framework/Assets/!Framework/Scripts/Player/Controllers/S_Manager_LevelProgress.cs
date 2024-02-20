using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class S_Manager_LevelProgress : MonoBehaviour {

    public static event EventHandler onReset;


    public S_CharacterTools tools;
    public Vector3 ResumePosition { get; set; }
    public Quaternion ResumeRotation { get; set; }
    Vector3 ResumeFace;
    public GameObject basePlayer;
    public Transform ResumeTransform;
    S_ActionManager Actions;
    S_PlayerPhysics Player;
    S_Handler_Camera Cam;
    S_PlayerInput Inp;
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

        Cam = basePlayer.GetComponent<S_Handler_Camera>();
        Actions = basePlayer.GetComponent<S_ActionManager>();
        Player = basePlayer.GetComponent<S_PlayerPhysics>();
        Inp = basePlayer.GetComponent<S_PlayerInput>();
        characterTransform = tools.CharacterAnimator.transform;
        ResumePosition = characterTransform.position;
        ResumeRotation = characterTransform.rotation;
        ResumeFace = characterTransform.forward;

    }

    void Update()
    {
        //LampDone.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);
        //LampDone.SetTextureOffset("_EmissionMap", new Vector2(0, -Time.time) * 3);

        if (readyForNextStage)
        {
            Player._moveInput = Vector3.zero;
            readyCount += Time.deltaTime;
            if(readyCount > 1.5f)
            {
                Actions.Action04Control.enabled = false;
                Color alpha = Color.black;
                Actions.Action04Control.FadeOutImage.color = Color.Lerp(Actions.Action04Control.FadeOutImage.color, alpha, Time.fixedTime * 0.1f);
            }
            if(readyCount > 2.6f)
            {
                LoadingScreenControl.StageName1 = NextLevelNameLeft;
                LoadingScreenControl.StageName2 = NextLevelNameRight;
                SceneManager.LoadScene(2);
            }
        }
    }

    void LateUpdate()
    {
        //if (!firstime)
        //{
        //    ResumePosition = transform.position;
        //    ResumeRotation = transform.rotation;
        //    ResumeFace = transform.forward;
        //    firstime = true;
        //}
    }

    
    public void ResetToCheckPoint()
    {
       
        Inp.LockInputForAWhile(20, true);
        StartCoroutine(Actions.lockAirMoves(20));
        Actions.ChangeAction(S_Enums.PlayerStates.Regular);

        tools.HomingTrailScript.emitTime = 0;
        tools.HomingTrailScript.emit = false;

        if (S_Interaction_Monitors.HasShield) 
		{
			S_Interaction_Monitors.HasShield = false;
		}

        transform.position = ResumePosition;
        //transform.rotation = ResumeRotation;
        characterTransform.forward = ResumeFace;
      

        Player.SetTotalVelocity(characterTransform.forward * 2);
        Actions.Action04.deadCounter = 0;

        Cam.Cam.Reversed = false;
        Cam.Cam.setBehindWithHeight();

    }

    public void RespawnObjects()
    {

        if(onReset != null)
        {
            Debug.LogWarning("Has begun respawning");
            onReset.Invoke(this, EventArgs.Empty);
        }
            
    }

    public void SetCheckPoint(Transform position)
    {
        ResumePosition = position.position;
        //ResumeRotation = position.rotation;
        ResumeFace = position.forward;
        ResumeTransform = position;

        if (Actions.eventMan != null) Actions.eventMan.AddDeathsPerCP();
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Checkpoint")
        {
            if (col.GetComponent<S_Data_Checkpoint>() != null)
            {
                //Set Object
                if (!col.GetComponent<S_Data_Checkpoint>().IsOn)
                {
                    if(Actions.eventMan != null)
                    {
                        Actions.eventMan.LogEvents(false, col.GetComponent<S_Data_Checkpoint>().checkPointName);
                    }

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
            if (Actions.eventMan != null)
            {
                StartCoroutine(Actions.eventMan.logEndEvents());
         
            }

            readyForNextStage = true;
			S_Interaction_Objects.RingAmount = 0;

			if (S_Interaction_Monitors.HasShield) 
			{
				S_Interaction_Monitors.HasShield = false;
			}

            StartCoroutine(endLevel(col));
			
        }
    }

    IEnumerator endLevel(Collider col)
    {
        for(int i = 0; i == 2; i++)
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
