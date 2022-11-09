using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Action06_Bounce : MonoBehaviour {

    ActionManager Action;
    Animator CharacterAnimator;
    PlayerBhysics Player;
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

			AssignStats();	
		}
        
	}

 

    public void InitialEvents()
    {

		HasBounced = false;

		////Debug.Log ("BounceDrop");
		sounds.BounceStartSound();
		BounceAvailable = false;
		Player.rb.velocity = new Vector3(Player.rb.velocity.x * BounceHaltFactor, 0f, Player.rb.velocity.z * BounceHaltFactor);
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

		Player.rb.velocity = new Vector3 (Player.rb.velocity.x, CurrentBounceAmount, Player.rb.velocity.z);
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
		Player.rb.velocity = new Vector3 (0f, Player.rb.velocity.y, 0f);
		CharacterAnimator.SetInteger("Action", 6);

		jumpBall.SetActive(false);

		Action.BouncePressed = false;
		Action.Action02.HomingAvailable = true;

		Input.LockInputForAWhile(20, false);

		HomingTrailScript.emitTime = 0;
		HomingTrailScript.emit = true;

		Action.ChangeAction(0);
	}

    void FixedUpdate()
    {
        bool raycasthit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 0.95f) + Player.negativeGHoverHeight, Player.Playermask);
		bool StompHit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 0.9f), Player.Playermask);
		bool groundhit = Player.Grounded || raycasthit;


        //End Action
        if (!raycasthit && HasBounced && Player.rb.velocity.y > 5f) { 
			
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

		else if(!groundhit && Player.rb.velocity.y == 0 && !HasBounced)
				Player.rb.velocity = new Vector3(Player.rb.velocity.x, -DropSpeed, Player.rb.velocity.z);

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
		DropSpeed = Tools.stats.DropSpeed;
		for (int i = 0; i < Tools.stats.BounceUpSpeeds.Count; i++)
		{
			BounceUpSpeeds.Add(Tools.stats.BounceUpSpeeds[i]);
		}
		BounceUpMaxSpeed = Tools.stats.BounceUpMaxSpeed;
		BounceConsecutiveFactor = Tools.stats.BounceConsecutiveFactor;
		BounceHaltFactor = Tools.stats.BounceHaltFactor;

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
