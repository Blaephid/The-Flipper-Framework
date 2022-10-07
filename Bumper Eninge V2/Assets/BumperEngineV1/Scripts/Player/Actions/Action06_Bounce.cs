using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Action06_Bounce : MonoBehaviour {

    ActionManager Action;
    Animator CharacterAnimator;
    PlayerBhysics Player;
	CharacterStats Stats;
	CharacterTools Tools;

	PlayerBinput Input;
	SonicSoundsControl sounds;
	VolumeTrailRenderer HomingTrailScript;
	GameObject jumpBall;

	[HideInInspector] public bool BounceAvailable;
	private bool HasBounced;
	private float OriginalBounceFactor;
	private float CurrentBounceAmount;
	[HideInInspector] public int BounceCount;

	//[SerializeField] public bool ShouldStomp;

	float DropSpeed;
	[HideInInspector] public List<float> BounceUpSpeeds;
	float BounceUpMaxSpeed;
	float BounceConsecutiveFactor;
	float BounceHaltFactor;
    

    Vector3 direction;
    RaycastHit hit;

    void Awake()
    {
		OriginalBounceFactor = BounceConsecutiveFactor;

		if (Player == null)
        {
			Tools = GetComponent<CharacterTools>();
			AssignTools();

			Stats = GetComponent<CharacterStats>();
			AssignStats();	
		}
        
	}

 

    public void InitialEvents()
    {

		HasBounced = false;

		////Debug.Log ("BounceDrop");
		sounds.BounceStartSound();
		BounceAvailable = false;
		Player.p_rigidbody.velocity = new Vector3(Player.p_rigidbody.velocity.x * BounceHaltFactor, 0f, Player.p_rigidbody.velocity.z * BounceHaltFactor);
		Player.AddVelocity (new Vector3 (0, -DropSpeed, 0));

		HomingTrailScript.emitTime = -1f;
		HomingTrailScript.emit = true;

		//Set Animator Parameters
		CharacterAnimator.SetInteger ("Action", 1);
		CharacterAnimator.SetBool ("isRolling", false);
		jumpBall.SetActive(true);
    }

   		
	private void Bounce(Vector3 normal)
	{
		Action.BouncePressed = false;
		Action.Action02.HomingAvailable = true;

		HasBounced = true;
		CurrentBounceAmount = BounceUpSpeeds [BounceCount];
		

		CurrentBounceAmount = Mathf.Clamp (CurrentBounceAmount, BounceUpSpeeds [BounceCount], BounceUpMaxSpeed);

		//HomingTrailScript.emitTime = (BounceCount +1) * 0.65f;
		HomingTrailScript.emitTime = CurrentBounceAmount / 60f;

		HomingTrailScript.emit = true;

		Player.p_rigidbody.velocity = new Vector3 (Player.p_rigidbody.velocity.x, CurrentBounceAmount, Player.p_rigidbody.velocity.z);
		Player.AddVelocity (Player.GroundNormal);

		sounds.BounceImpactSound ();

		//Set Animator Parameters
		CharacterAnimator.SetInteger ("Action", 1);
		CharacterAnimator.SetBool ("isRolling", false);
		jumpBall.SetActive(false);

		if (BounceCount < BounceUpSpeeds.Count - 1) {
			BounceCount++;
		}

	}

	private void Stomp()
    {
		Player.p_rigidbody.velocity = new Vector3 (0f, Player.p_rigidbody.velocity.y, 0f);
		CharacterAnimator.SetInteger("Action", 6);

		jumpBall.SetActive(false);

		Action.BouncePressed = false;
		Action.Action02.HomingAvailable = true;

		Input.LockInputForAWhile(20, false);

		HomingTrailScript.emitTime = 0;
		HomingTrailScript.emit = true;

		Action.ChangeAction(0);
	}

    void Update()
    {
        bool raycasthit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 0.95f) + Player.negativeGHoverHeight, Player.Playermask);
		bool StompHit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 0.9f), Player.Playermask);
		bool groundhit = Player.Grounded || raycasthit;

        //End Action
        if (!raycasthit && HasBounced && Player.p_rigidbody.velocity.y > 10f) { 
			
			HasBounced = false;

			////Debug.Log ("BackToIdleJump");

			Action.ChangeAction (0);
		} 

		else if (groundhit && !HasBounced) 
		{
			if (Action.SkidPressed)
            {
				Stomp();
			}

			else
			{
				if (!raycasthit)
				{
					//Debug.Log("Ground Bounce " + Player.GroundNormal);
					Bounce(Player.GroundNormal);
				}
				else
				{
					//Debug.Log("RaycastHitBounce " + hit.normal);
					//transform.position = hit.point;
					Bounce(hit.normal);
				}
			}
		}

        //Stomp
        //else if (StompHit && !HasBounced && Action.SkidPressed)
        //{
        //    Player.p_rigidbody.velocity = Vector3.zero;
        //    CharacterAnimator.SetInteger("Action", 6);

        //    Action.BouncePressed = false;
        //    Action.Action02.HomingAvailable = true;

        //    Input.LockInputForAWhile(20, false);

        //    HomingTrailScript.emitTime = 0;
        //    HomingTrailScript.emit = true;

        //    Action.ChangeAction(0);
        //}

    }
    
	private void AssignStats()
    {
		DropSpeed = Stats.DropSpeed;
		for (int i = 0; i < Stats.BounceUpSpeeds.Count; i++)
		{
			BounceUpSpeeds.Add(Stats.BounceUpSpeeds[i]);
		}
		BounceUpMaxSpeed = Stats.BounceUpMaxSpeed;
		BounceConsecutiveFactor = Stats.BounceConsecutiveFactor;
		BounceHaltFactor = Stats.BounceHaltFactor;

	}

	private void AssignTools()
    {
		Player = GetComponent<PlayerBhysics>();
		Input = GetComponent<PlayerBinput>();
		Action = GetComponent<ActionManager>();

		CharacterAnimator = Tools.CharacterAnimator;
		sounds = Tools.SoundControl;
		HomingTrailScript = Tools.HomingTrailScript;
		jumpBall = Tools.JumpBall;
	}
}
