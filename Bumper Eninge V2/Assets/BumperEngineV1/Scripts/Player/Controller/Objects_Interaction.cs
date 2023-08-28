using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Objects_Interaction : MonoBehaviour {

    CharacterTools Tools;

    [Header("For Rings, Springs and so on")]

    PlayerBhysics Player;
    HedgeCamera Cam;
    Animator CharacterAnimator;
    SonicSoundsControl Sounds;
    ActionManager Actions;
    PlayerBinput Inp;
    Attack attack;

    GameObject JumpBall;
    
    Spring_Proprieties spring;
    int springAmm;
    

    public GameObject RingCollectParticle;
    public Material SpeedPadTrack;
    public Material DashRingMaterial;

    [Header("Enemies")]

    
    [HideInInspector] public float HomingBouncingPower;

    public bool updateTargets { get; set; }

    [HideInInspector] public float EnemyDamageShakeAmmount;
    [HideInInspector] public float EnemyHitShakeAmmount;

    [Header("UI objects")]

    public TextMeshProUGUI RingsCounter;
    public TextMeshProUGUI SpeedCounter;
    public HintBox HintBox;


    public static int RingAmount { get; set; }
    [HideInInspector] public int CurrentRings;
    [HideInInspector] public float DisplaySpeed;

    MovingPlatformControl Platform;
    Vector3 TranslateOnPlatform;
    public Color DashRingLightsColor;

    private void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }

    }

    //Displays rings and speed on UI
    private void LateUpdate()
    {
        UpdateSpeed();

        CurrentRings = RingAmount;
        RingsCounter.text = ": " + RingAmount;
    }

    void UpdateSpeed()
    {
        if (Actions.Action == ActionManager.States.Regular)
        {
            //DisplaySpeed = Player.SpeedMagnitude;
            DisplaySpeed = Player.HorizontalSpeedMagnitude;
        }
        
        else if(Actions.Action == ActionManager.States.Rail)
        {
            DisplaySpeed = Actions.Action05.PlayerSpeed;
        }
        else if (Actions.Action == ActionManager.States.WallRunning)
        {
            if (Actions.Action12.RunningSpeed > Actions.Action12.ClimbingSpeed)
            DisplaySpeed = Actions.Action12.RunningSpeed;
            else
                DisplaySpeed = Actions.Action12.ClimbingSpeed;

        }
        else if (Actions.Action == ActionManager.States.Path)
        {
            DisplaySpeed = Actions.Action10.PlayerSpeed;
        }
        else
        {
             DisplaySpeed = Player.HorizontalSpeedMagnitude;           
        }

        if (SpeedCounter != null && Player.SpeedMagnitude > 10f) SpeedCounter.text = DisplaySpeed.ToString("F0");
        else if (SpeedCounter != null && DisplaySpeed < 10f) SpeedCounter.text = "0"; 
    }

    void Update()
    {

        if (updateTargets)
        {
            //HomingAttackControl.UpdateHomingTargets();
			if (Actions.Action02 != null) {
				if (Actions.Action02 != null) {
			Actions.Action02.HomingAvailable = true;
		}
			}
            updateTargets = false;
        }



        //Set speed pad trackpad's offset
        SpeedPadTrack.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);
        DashRingMaterial.SetColor("_EmissionColor", (Mathf.Sin(Time.time * 15) * 1.3f) * DashRingLightsColor);
    }

	

    void FixedUpdate()
    {
        if(Platform != null)
        {
            transform.position += (-Platform.TranslateVector);
        }
        if (!Player.Grounded)
        {
            Platform = null;
        }


    }

    IEnumerator setRailSpeed(float speed, bool set, float addSpeed, bool backwards)
    {
        for(int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();

            if(set)
            {
                if (Actions.Action05.PlayerSpeed < speed)
                {
                    Actions.Action05.PlayerSpeed = speed;
                    Actions.Action05.Boosted = true;
                    Actions.Action05.boostTime = 0.7f;
                    
                }
                if (backwards)
                    Actions.Action05.backwards = true;
                else
                    Actions.Action05.backwards = false;
            }
            else if (Actions.Action == ActionManager.States.Rail)
            {

                Actions.Action05.PlayerSpeed += addSpeed / 2;
                Actions.Action05.Boosted = true;
                Actions.Action05.boostTime = 0.7f;
                if (backwards)
                    Actions.Action05.backwards = true;
                else
                    Actions.Action05.backwards = false;

                i = 3;
                
            }
        }

    }

    public void OnTriggerEnter(Collider col)
    {
        //Speed Pads Collision
        if(col.tag == "SpeedPad")
        {
            SpeedPadData pad = col.GetComponent<SpeedPadData>();

            col.GetComponent<AudioSource>().Play();

            JumpBall.SetActive(false);
            if (Actions.Action08 != null)
            {
                if (Actions.Action08.DropEffect.isPlaying == true)
                {
                    Actions.Action08.DropEffect.Stop();
                }
            }

            if (pad.onRail)
             {

                if (Actions.Action != ActionManager.States.Rail)
                {
                    transform.position = col.GetComponent<SpeedPadData>().positionToLockTo.position;
                }
                else
                {
                    StartCoroutine(setRailSpeed(pad.Speed, pad.setSpeed, pad.addSpeed, pad.railBackwards));
                }        
                return;
            }
                             

            else if(!col.GetComponent<SpeedPadData>().path)
            {
                
                Actions.Action02.HomingAvailable = true;

                transform.rotation = Quaternion.identity;
                //ResetPlayerRotation

                Vector3 lockpos;
                if (col.GetComponent<SpeedPadData>().positionToLockTo != null)
                    lockpos = col.GetComponent<SpeedPadData>().positionToLockTo.position;
                else
                    lockpos = col.transform.position;


                if (pad.LockToDirection)
                {
                    float speed = col.GetComponent<SpeedPadData>().Speed;
                    if (speed < Player.HorizontalSpeedMagnitude)
                        speed = Player.HorizontalSpeedMagnitude;

                    if(!pad.isDashRing)
                        StartCoroutine(applyForce(col.transform.forward * speed, lockpos, 1));
                    else
                        StartCoroutine(applyForce(col.transform.forward * col.GetComponent<SpeedPadData>().Speed, lockpos));
                }
                else
                {
                    Player.AddVelocity(col.transform.forward * col.GetComponent<SpeedPadData>().Speed);

                    if (col.GetComponent<SpeedPadData>().Snap)
                    {
                        transform.position = lockpos;
                    }
                }

               
                if (pad.isDashRing)
                {
                                  
                    Actions.Action00.cancelCoyote();
                    Actions.ChangeAction(ActionManager.States.Regular);
                    CharacterAnimator.SetBool("Grounded", false);
                    CharacterAnimator.SetInteger("Action", 0);

                    if (pad.lockAirMoves)
                    {
                        StopCoroutine(Actions.lockAirMoves(pad.lockAirMovesTime));
                        StartCoroutine(Actions.lockAirMoves(pad.lockAirMovesTime));
                    }

                    if (pad.lockGravity != Vector3.zero)
                    {
                        StartCoroutine(lockGravity(pad.lockGravity));
                    }
                    
                    

                }
                else
                {
                    transform.up = col.transform.up;
                    CharacterAnimator.transform.forward = col.transform.forward;
                }

                if (pad.LockControl)
                {
                    Inp.LockInputForAWhile(pad.LockControlTime, true);
                    if(pad.setInputForwards)
                    {
                        Actions.moveX = 0;
                        Actions.moveY = 1;
                    }
                }
                if (pad.AffectCamera)
                {
                    Vector3 dir = col.transform.forward;
                    Cam.SetCamera(dir, 2.5f, 20, 5f, 1);
                    
                }

            }
        }

        //Rings Collision
        if (col.tag == "Ring")
        {
			Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
			Destroy(col.gameObject);
			StartCoroutine(IncreaseRing ());
            
            
        }
        else if (col.tag == "Ring Road")
        {
            //Actions.Action07Control.UpdateHomingTargets();
            Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
            Destroy(col.gameObject);
            StartCoroutine(IncreaseRing());
        }
        else if (col.tag == "MovingRing")
        {
            if (col.GetComponent<MovingRing>() != null)
            {
                if (col.GetComponent<MovingRing>().colectable)
                {
					StartCoroutine(IncreaseRing ());
                    Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
                    Destroy(col.gameObject);
                }
            }
        }

		//Switch
		if(col.tag == "Switch")
		{	
			if (col.GetComponent<Switch_Properties> () != null) {
				col.GetComponent<Switch_Properties> ().Activate ();
			}
		}

        //Hazard
        if(col.tag == "Hazard")
        {
			JumpBall.SetActive(false);
			if (Actions.Action08 != null) {
				if (Actions.Action08.DropEffect.isPlaying == true) {
					Actions.Action08.DropEffect.Stop ();
				}
			}
            DamagePlayer();
            HedgeCamera.Shakeforce = EnemyDamageShakeAmmount;
        }

        //Enemies
        if (col.tag == "Enemy")
        {
            HedgeCamera.Shakeforce = EnemyHitShakeAmmount;
            //If 1, destroy, if not, take damage.
            if (Actions.Action == ActionManager.States.SpinCharge)
            {
                attack.AttackThing(col, "SpinDash");
                
            }
            if (CharacterAnimator.GetInteger("Action") == 1 || CharacterAnimator.GetInteger("Action") == 11)
            {
                attack.AttackThing(col, "SpinJump");

                
            }


            else if(Actions.Action != ActionManager.States.SpinCharge)
            {
                DamagePlayer();
            }
        }

        ////Monitors
        if (col.tag == "Monitor")
        {
            if (CharacterAnimator.GetInteger("Action") == 1)
            {
                col.GetComponentInChildren<BoxCollider>().enabled = false;

                attack.AttackThing(col, "SpinJump", "Monitor");
            }


        }


        //Spring Collision

        if (col.tag == "Spring")
        {
            Actions.Action00.cancelCoyote();
            Player.GravityAffects = true;

            if (Actions.Action == ActionManager.States.Homing || Actions.PreviousAction == ActionManager.States.Homing)
                Player.HomingDelay = Tools.stats.HomingSuccessDelay;

            JumpBall.SetActive(false);
			if (Actions.Action08 != null) {
				if (Actions.Action08.DropEffect.isPlaying == true) {
					Actions.Action08.DropEffect.Stop ();
				}
			}


            if (col.GetComponent<Spring_Proprieties>() != null)
            {

                spring = col.GetComponent<Spring_Proprieties>();

                if (spring.LockControl)
                {
                    Inp.LockInputForAWhile(spring.LockTime, false);
                }

                if(spring.lockAirMoves)
                {
                    StopCoroutine(Actions.lockAirMoves(spring.lockAirMovesTime));
                    StartCoroutine(Actions.lockAirMoves(spring.lockAirMovesTime));
                }

                if(spring.lockGravity != Vector3.zero)
                {
                    StartCoroutine(lockGravity(spring.lockGravity));
                }

                Actions.ChangeAction(ActionManager.States.Regular);
                

                if (col.GetComponent<AudioSource>()) { col.GetComponent<AudioSource>().Play(); }
                CharacterAnimator.SetInteger("Action", 0);
                CharacterAnimator.SetBool("Grounded", false);

                if (Actions.Action02 != null) {
				    Actions.Action02.HomingAvailable = true;
			    }

                if (spring.anim != null)
                     spring.anim.SetTrigger("Hit");

                if (spring.IsAdditive)
                {
                    Vector3 newVelocity = new Vector3(Player.rb.velocity.x, 0f, Player.rb.velocity.z);
                    newVelocity = (newVelocity * 0.8f) + (spring.transform.up * spring.SpringForce);
                    StartCoroutine(applyForce(newVelocity, spring.BounceCenter.position));
                }
                    

                else
                {
                    StartCoroutine(applyForce(spring.transform.up * spring.SpringForce, spring.BounceCenter.position));
                }

                transform.position = spring.BounceCenter.position;

            }
        }

		else if (col.tag == "Bumper")
		{
            if (Actions.Action == ActionManager.States.Homing || Actions.PreviousAction == ActionManager.States.Homing)
                Player.HomingDelay = Tools.stats.HomingSuccessDelay;

            JumpBall.SetActive(false);
			if (Actions.Action08 != null) {
				if (Actions.Action08.DropEffect.isPlaying == true) {
					Actions.Action08.DropEffect.Stop ();
				}
			}
			
		}

		//CancelHoming
		else if (col.tag == "CancelHoming") 
		{
			if (Actions.Action == ActionManager.States.Homing || Actions.PreviousAction == ActionManager.States.Homing)
            {

				Vector3 newSpeed = new Vector3(1, 0, 1);

				Actions.ChangeAction (ActionManager.States.Regular);
				newSpeed = new Vector3(0, HomingBouncingPower, 0);
				////Debug.Log (newSpeed);
				Player.rb.velocity = newSpeed;
				Player.transform.position = col.ClosestPoint (Player.transform.position);
				if (Actions.Action02 != null) {
					Actions.Action02.HomingAvailable = true;
				}

                Player.HomingDelay = Tools.stats.HomingSuccessDelay;
            }
		}

        else if(col.tag == "Wind")
        {
            if(col.GetComponent<updraft>())
            {
                if (Actions.Action == ActionManager.States.Hovering)
                {
                    Actions.Action13.updateHover(col.GetComponent<updraft>());
                }
                else
                {
                    Actions.Action13.InitialEvents(col.GetComponent<updraft>());
                    Actions.ChangeAction(ActionManager.States.Hovering);
                }
            }
                                
        }

        else if (col.tag == "HintRing")
        {
            HintRingActor hintRing = col.GetComponent<HintRingActor>();
            //if (!HintBox.IsShowing)
            //{
            //    HintBox.ShowHint(hintRing.hintText, hintRing.hintDuration);
            //    hintRing.hintSound.Play();
            //}

            if (col.gameObject != HintBox.currentHint)
            {
                HintBox.currentHint = col.gameObject;

                if (Actions.usingMouse)
                    HintBox.ShowHint(hintRing.hintText, hintRing.hintDuration, col.gameObject);
                else
                    HintBox.ShowHint(hintRing.hintTextGamePad, hintRing.hintDuration, col.gameObject);
                hintRing.hintSound.Play();


                if (Actions.eventMan != null)
                {
                    foreach (GameObject HR in Actions.eventMan.hintRings)
                    {
                        if (col.gameObject == HR)
                            return;
                    }
                    Actions.eventMan.hintRings.Add(col.gameObject);
                    Actions.eventMan.hintRingsHit += 1;
                }

            }

        }

    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "Wind")
        {
            Actions.Action13.inWind = false;
        }
    }

    public void OnTriggerStay(Collider col)
    {
        //Hazard
        if (col.tag == "Hazard")
        {
            DamagePlayer();
        }

        if (col.gameObject.tag == "MovingPlatform")
        {
            Platform = col.gameObject.GetComponent<MovingPlatformControl>();
        }
        else
        {
            Platform = null;
        }

       

    }

	private IEnumerator IncreaseRing()
	{
		int ThisFramesRingCount = RingAmount;
		RingAmount++;
		yield return new WaitForEndOfFrame ();
		if (RingAmount > ThisFramesRingCount + 1) 
		{
			RingAmount--;
		}
			
	}

    private IEnumerator lockGravity(Vector3 newGrav)
    {

        Player.fallGravity = newGrav;
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (Player.Grounded)
                break;
        }

        Player.fallGravity = Player.StartFallGravity;
    }

    private IEnumerator applyForce(Vector3 force, Vector3 position, int frames = 3)
    {
        
        for(int i = 0; i < frames; i++)
        {
            transform.position = position;
            Player.rb.velocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
        }

        Actions.ChangeAction(ActionManager.States.Regular);
        transform.position = position;
        Player.rb.velocity = force;

    }
    public void DamagePlayer()
    {
        if (!Actions.Action04Control.IsHurt && Actions.Action != ActionManager.States.Hurt)
        {

            if (!Monitors_Interactions.HasShield)
            {
                if (RingAmount > 0)
                {
                    //LoseRings
                    Sounds.RingLossSound();
                    Actions.Action04Control.GetHurt();
                    Actions.ChangeAction(ActionManager.States.Hurt);
                    Actions.Action04.InitialEvents();
                }
                if (RingAmount <= 0)
                {
                    //Die
                    if (!Actions.Action04Control.isDead)
                    {
                        Sounds.DieSound();
                        Actions.Action04Control.isDead = true;
                        Actions.ChangeAction(ActionManager.States.Hurt);
                        Actions.Action04.InitialEvents();
                    }
                }
            }
            if (Monitors_Interactions.HasShield)
            {
                //Lose Shield
                Sounds.SpikedSound();
                Monitors_Interactions.HasShield = false;
                Actions.ChangeAction(ActionManager.States.Hurt);
                Actions.Action04.InitialEvents();
            }
        }
    }


    private void AssignStats()
    {
        HomingBouncingPower = Tools.coreStats.HomingBouncingPower;
        EnemyDamageShakeAmmount = Tools.coreStats.EnemyDamageShakeAmmount;
        EnemyHitShakeAmmount = Tools.coreStats.EnemyHitShakeAmmount;
    }

    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Cam = GetComponent<CameraControl>().Cam;
        Actions = GetComponent<ActionManager>();
        Inp = GetComponent<PlayerBinput>();
        attack = GetComponent<Attack>();

        CharacterAnimator = Tools.CharacterAnimator;
        Sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;


    }
}
