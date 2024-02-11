using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class S_Action06_Bounce : MonoBehaviour {

    S_ActionManager Action;
    Animator CharacterAnimator;
    S_PlayerPhysics Player;
	S_CharacterTools Tools;

	S_PlayerInput Input;
	S_Control_SoundsPlayer sounds;
	S_VolumeTrailRenderer HomingTrailScript;
	GameObject jumpBall;

	[HideInInspector] public bool BounceAvailable;
	private bool HasBounced;

	private float CurrentBounceAmount;
	[HideInInspector] public int BounceCount;

	//[SerializeField] public bool ShouldStomp;

	float _dropSpeed_;
	[HideInInspector] public List<float> BounceUpSpeeds;
	float _bounceUpMaxSpeed_;
	float _bounceConsecutiveFactor_;
	float _bounceHaltFactor_;
	float _bounceCoolDown_;

	float memoriseSpeed;
	float nextSpeed;

	float hitHeight;

    Vector3 direction;
    RaycastHit hit;

    void Awake()
    {

		if (Player == null)
        {
			Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();	
		}
        
	}

 

    public void InitialEvents()
    {
		if(!Action.lockBounce)
        {
	
			HasBounced = false;
			memoriseSpeed = Player._horizontalSpeedMagnitude;
			nextSpeed = memoriseSpeed;


			sounds.BounceStartSound();
			BounceAvailable = false;
			Player.rb.velocity = new Vector3(Player.rb.velocity.x * _bounceHaltFactor_, 0f, Player.rb.velocity.z * _bounceHaltFactor_);
			Player.AddVelocity(new Vector3(0, -_dropSpeed_, 0));

			HomingTrailScript.emitTime = -1f;
			HomingTrailScript.emit = true;

			//Set Animator Parameters
			CharacterAnimator.SetInteger("Action", 1);
			CharacterAnimator.SetBool("isRolling", false);
			jumpBall.SetActive(true);
		}
    }

   		
	private void Bounce(Vector3 normal)
	{

		hitHeight = transform.position.y;

		Action.BouncePressed = false;
		Player.SetIsGrounded(false);
		Action.Action02.HomingAvailable = true;

		HasBounced = true;
		CurrentBounceAmount = BounceUpSpeeds [BounceCount];
		

		CurrentBounceAmount = Mathf.Clamp (CurrentBounceAmount, BounceUpSpeeds [BounceCount], _bounceUpMaxSpeed_);

		//HomingTrailScript.emitTime = (BounceCount +1) * 0.65f;
		HomingTrailScript.emitTime = CurrentBounceAmount / 60f;

		HomingTrailScript.emit = true;

		Vector3 newVec;

		
		if(Player._horizontalSpeedMagnitude < 20)
        {
			newVec = CharacterAnimator.transform.forward;
			newVec *= 20;
		}
		else if (nextSpeed > Player._horizontalSpeedMagnitude)
		{
			newVec = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z).normalized;
			newVec *= nextSpeed;
		}
		else
			newVec = Player.rb.velocity;

		Player.rb.velocity = new Vector3 (newVec.x, CurrentBounceAmount, newVec.z);
		Player.AddVelocity (Player._groundNormal);

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

		Action.ChangeAction(S_Enums.PlayerStates.Regular);
	}


    void FixedUpdate()
    {

		bool raycasthit = Physics.SphereCast(transform.position, 0.5f, -transform.up, out hit, (Player._speedMagnitude * Time.deltaTime * 0.75f) + Player._negativeGHoverHeight_, Player._Groundmask_);
        //bool raycasthit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 0.95f) + Player.negativeGHoverHeight, Player.Playermask);
		bool groundhit = Player._isGrounded || raycasthit;

		if (nextSpeed > memoriseSpeed / 2)
			nextSpeed /= 1.0005f;


        //End Action
        if (!raycasthit && HasBounced && Player.rb.velocity.y > 4f) { 
			
			HasBounced = false;

			float coolDown = _bounceCoolDown_;
			//coolDown -= 0.75f * (int)(Player.HorizontalSpeedMagnitude / 20);
			//coolDown = Mathf.Clamp(coolDown, 3, 6);

			//StartCoroutine(Action.lockBounceOnly(coolDown));
			Action.ChangeAction (S_Enums.PlayerStates.Regular);
		} 

		else if ((groundhit && !HasBounced) || (!groundhit && Player.rb.velocity.y > _dropSpeed_ * 0.4f && !HasBounced)) 
		{
			
			//if (Action.SkidPressed)
   //         {
			//	Stomp();
			//}

			if(true)
			{
				if (Player._isGrounded)
				{
					//Debug.Log("Ground Bounce " + Player.GroundNormal);
					Bounce(Player._groundNormal);
				}
				else if (raycasthit)
				{
					//Debug.Log("RaycastHitBounce " + hit.normal);
					//transform.position = hit.point;
					Bounce(hit.normal);
				}
				else
                {
					
					Bounce(Vector3.up);
				}
			}
		}
		else if(Player.rb.velocity.y > _dropSpeed_ * 0.8f)
        {
			Player.rb.velocity = new Vector3(Player.rb.velocity.x, -_dropSpeed_, Player.rb.velocity.z);
        }

    }
    
	private void AssignStats()
    {
		_dropSpeed_ = Tools.Stats.BounceStats.dropSpeed;
		for (int i = 0; i < Tools.Stats.BounceStats.listOfBounceSpeeds.Count; i++)
		{
			BounceUpSpeeds.Add(Tools.Stats.BounceStats.listOfBounceSpeeds[i]);
		}
		_bounceUpMaxSpeed_ = Tools.Stats.BounceStats.bounceUpMaxSpeed;
		_bounceCoolDown_ = Tools.Stats.BounceStats.bounceCoolDown;
		_bounceConsecutiveFactor_ = Tools.Stats.BounceStats.bounceConsecutiveFactor;
		_bounceHaltFactor_ = Tools.Stats.BounceStats.bounceHaltFactor;

	}

	private void AssignTools()
    {
		Player = GetComponent<S_PlayerPhysics>();
		Input = GetComponent<S_PlayerInput>();
		Action = GetComponent<S_ActionManager>();

		CharacterAnimator = Tools.CharacterAnimator;
		sounds = Tools.SoundControl;
		HomingTrailScript = Tools.HomingTrailScript;
		jumpBall = Tools.JumpBall;
	}
}
