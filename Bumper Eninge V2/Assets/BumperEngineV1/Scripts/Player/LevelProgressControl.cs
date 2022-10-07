using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelProgressControl : MonoBehaviour {

    public Vector3 ResumePosition { get; set; }
    public Quaternion ResumeRotation { get; set; }
    public GameObject basePlayer;
    public Transform ResumeTransform;
    ActionManager Actions;
    PlayerBhysics Player;
    CameraControl Cam;
    public GameObject CurrentCheckPoint { get; set; }

    public Material LampDone;
    public int LevelToGoNext = 0;
    public string NextLevelNameLeft;
    public string NextLevelNameRight;
    public AudioClip GoalRingTouchingSound;

    bool readyForNextStage = false;
    float readyCount = 0;

    bool firstime = false;

    void Start () {

        Cam = basePlayer.GetComponent<CameraControl>();
        Actions = basePlayer.GetComponent<ActionManager>();
        Player = basePlayer.GetComponent<PlayerBhysics>();

    }

    void Update()
    {
        LampDone.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);
        LampDone.SetTextureOffset("_EmissionMap", new Vector2(0, -Time.time) * 3);

        if (readyForNextStage)
        {
            Player.MoveInput = Vector3.zero;
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
        if (!firstime)
        {
            ResumePosition = transform.position;
            ResumeRotation = transform.rotation;
            firstime = true;
        }
    }

    public void ResetToCheckPoint()
    {
        //Debug.Log("Reset");

		Objects_Interaction.RingAmount = 0;

		if (Monitors_Interactions.HasShield) 
		{
			Monitors_Interactions.HasShield = false;
		}

        transform.position = ResumePosition;
        transform.rotation = ResumeRotation;
        Actions.Action04.deadCounter = 0;
        var dir = -ResumeTransform.forward;
        dir.y = 0.2f;
        //Cam.Cam.SetCamera(-9);
    }
    public void SetCheckPoint(Transform position)
    {
        ResumePosition = position.position;
        ResumeRotation = position.rotation;
        ResumeTransform = position;
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Checkpoint")
        {
            if (col.GetComponent<CheckPointData>() != null)
            {
                //Set Object
                if (!col.GetComponent<CheckPointData>().IsOn)
                {
                    col.GetComponent<CheckPointData>().IsOn = true;
                    col.GetComponent<AudioSource>().Play();
                    foreach (Animator anim in col.GetComponent<CheckPointData>().Animators)
                    {
                        anim.SetTrigger("Open");
                    }
                    col.GetComponent<CheckPointData>().Laser.SetActive(false);
                    SetCheckPoint(col.GetComponent<CheckPointData>().CheckPos);
                    CurrentCheckPoint = col.gameObject;
                    //CurrentCheckPointTimer = GameTimer;
                }

            }
        }
        if (col.tag == "GoalRing")
        {
            readyForNextStage = true;
			Objects_Interaction.RingAmount = 0;

			if (Monitors_Interactions.HasShield) 
			{
				Monitors_Interactions.HasShield = false;
			}
			SceneManager.LoadScene("StageCompleteScreen");
		//	StageConpleteControl.LevelToGoNext = SceneManager.GetActiveScene ().buildIndex + 1;
            col.GetComponent<AudioSource>().clip = GoalRingTouchingSound;
            col.GetComponent<AudioSource>().loop = false;
            col.GetComponent<AudioSource>().Play();
        }
    }
}
