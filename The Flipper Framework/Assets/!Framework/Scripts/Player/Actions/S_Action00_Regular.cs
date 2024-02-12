using UnityEngine;
using System.Collections;

public class S_Action00_Regular : MonoBehaviour {

    private Animator CharacterAnimator;
	S_CharacterTools Tools;
    S_PlayerPhysics Player;
	S_PlayerInput Input;
    S_ActionManager Actions;
	S_Handler_Camera Cam;
	S_Handler_quickstep quickstepManager;
    S_Control_SoundsPlayer sounds;
	GameObject characterCapsule;
	GameObject _rollingCapsule_;

    public float skinRotationSpeed;
    S_Action01_Jump JumpAction;
    Quaternion CharRot;

	float _MaximumSpeed_; //The max amount of speed you can be at to perform a Spin Dash
	float _MaximumSlope_; //The highest slope you can be on to Spin Dash

	float _SpeedToStopAt_;

	float _SkiddingStartPoint_;


	bool _CanDashDuringFall_;

	RaycastHit hit;

	AnimationCurve _CoyoteTimeBySpeed_;
	bool coyoteInEffect = false;
	Vector3 coyoteRememberDir;
	float coyoteRememberSpeed;
	bool inCoyote = false;

	//Used to prevent rolling sound from constantly playing.
	[HideInInspector] public bool Rolling = false;
	[HideInInspector] public float rollCounter;
	float minRollTime = 0.3f;

    void Awake()
    {
		Tools = GetComponent<S_CharacterTools>();
    }

    private void Start()
    {
        AssignTools();
		AssignStats();
    }

    private void OnDisable()
    {
		//cancelCoyote();
    }

    void FixedUpdate()
    {

		if(Player._speedMagnitude < 15 && Player._moveInput == Vector3.zero && Player._isGrounded)
		{
			Player.b_normalSpeed = 0;
			Player._RB.velocity *= 0.90f;
			Actions.skid._hasSked = false;

		}

		//Skidding

		if (Player._isGrounded)
			Actions.skid.RegularSkid();
		else
			Actions.skid.jumpSkid();

		//Set Homing attack to true
		if (Player._isGrounded) 
		{
			readyCoyote();

			if (Actions.Action02 != null) {
			Actions.Action02.HomingAvailable = true;
			}

			if (Actions.Action06.BounceCount > 0) {
				Actions.Action06.BounceCount = 0;
			}
				
		}
		else if(!inCoyote)
        {
			StartCoroutine(CoyoteTime());
        }

		

    }

	public void readyCoyote()
    {
		inCoyote = false;
		coyoteInEffect = true;
		if (Player._isGrounded)
			coyoteRememberDir = Player._groundNormal;
		else
			coyoteRememberDir = transform.up;
		coyoteRememberSpeed = Player._RB.velocity.y;
	}

    void Update()
    {	

        //Set Animator Parameters
        if (Player._isGrounded) { CharacterAnimator.SetInteger("Action", 0); }
        CharacterAnimator.SetFloat("YSpeed", Player._RB.velocity.y);
		CharacterAnimator.SetFloat("XZSpeed", Mathf.Abs((Player._RB.velocity.x+Player._RB.velocity.z)/2));
        CharacterAnimator.SetFloat("GroundSpeed", Player._RB.velocity.magnitude);
		CharacterAnimator.SetFloat("HorizontalInput", Actions.moveX *Player._RB.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player._isGrounded);
        CharacterAnimator.SetFloat("NormalSpeed", Player.b_normalSpeed + _SkiddingStartPoint_);

		//Set Character Animations and position1
		CharacterAnimator.transform.parent = null;
        
        //Set Skin Rotation
        if (Player._isGrounded)
		{
			Vector3 releVec = Player.GetRelevantVec(Player._RB.velocity);
			Vector3 newForward = Player._RB.velocity - transform.up * Vector3.Dot(Player._RB.velocity, transform.up);
			Debug.DrawRay(transform.position, newForward.normalized * 5, Color.yellow);
			//newForward = releVec - transform.up * Vector3.Dot(releVec, transform.up);

            if (newForward.magnitude < 0.1f)
            {
                newForward = CharacterAnimator.transform.forward;
            }

			CharRot = Quaternion.LookRotation(newForward, transform.up);
			CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);

           // CharRot = Quaternion.LookRotation( Player.rigidbody.velocity, transform.up);
           // CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
        }
        else
        {
			Vector3 releVec = Player.GetRelevantVec(Player._RB.velocity);
			Vector3 VelocityMod = new Vector3(releVec.x, 0, releVec.z);
			//VelocityMod = Player.rb.velocity;

			Vector3 newForward = Player._RB.velocity - transform.up * Vector3.Dot(Player._RB.velocity, transform.up);
			Debug.DrawRay(transform.position, newForward.normalized * 5, Color.yellow);
			if (VelocityMod != Vector3.zero)
            {
				//Quaternion CharRot = Quaternion.LookRotation(VelocityMod, -Player.fallGravity.normalized);
				Quaternion CharRot = Quaternion.LookRotation(newForward, transform.up);
				CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
			}
   
        }

		handleInputs();

	}

	public void Curl()
    {
		if (!Rolling)
		{
			if (Actions.eventMan != null) Actions.eventMan.RollsPerformed += 1;
			sounds.SpinningSound();
			_rollingCapsule_.SetActive(true);
			characterCapsule.SetActive(false);
			//rollCounter = 0f;
		}
		Player._isRolling = true;
		Rolling = true;
	}

	void unCurl()
    {
		CapsuleCollider col = _rollingCapsule_.GetComponent<CapsuleCollider>();


		if (!Physics.Raycast(col.transform.position + col.center, CharacterAnimator.transform.up, 4))
		{
			characterCapsule.SetActive(true);
			_rollingCapsule_.SetActive(false);
			rollCounter = 0f;
			Rolling = false;
			Player._isRolling = false;

		}

	}

	void handleInputs()
    {

		//Jump
		if (Actions.JumpPressed && (Player._isGrounded || coyoteInEffect))
		{

			if (Player._isGrounded)
				JumpAction.InitialEvents(Player._groundNormal, true, Player._RB.velocity.y);
			else
				JumpAction.InitialEvents(coyoteRememberDir, true, coyoteRememberSpeed);

			Actions.ChangeAction(S_Enums.PlayerStates.Jump);
		}

		//Set Camera to back
		if (Actions.CamResetPressed)
		{
			if (Actions.moveVec == Vector2.zero && Player.b_normalSpeed < 5f)
				Cam.Cam.FollowDirection(6, 14f, -10, 0);
		}

		

		//Do Spindash
		if (Actions.spinChargePressed && Player._isGrounded && Player._groundNormal.y > _MaximumSlope_ && Player._horizontalSpeedMagnitude < _MaximumSpeed_)
		{
			Actions.ChangeAction(S_Enums.PlayerStates.SpinCharge);
			Actions.Action03.InitialEvents();
		}

        //Check if rolling
        if (Player._isGrounded && Player._isRolling) 
		{ 
			CharacterAnimator.SetInteger("Action", 1); 
		}
        CharacterAnimator.SetBool("isRolling", Player._isRolling);

        //Change to rolling state
        if (Actions.RollPressed && Player._isGrounded)
		{
			Curl();
		}

		//Exit rolling state
		if ((!Actions.RollPressed && rollCounter > minRollTime) | !Player._isGrounded)
		{
			unCurl();
		}

		if (Rolling)
			rollCounter += Time.deltaTime;

		/////Quickstepping
		///
		//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
		if (Actions.RightStepPressed)
		{
			quickstepManager.pressRight();
		}
		else if (Actions.LeftStepPressed)
		{
			quickstepManager.pressLeft();
		}

		//Enable Quickstep right or left
		if (Actions.RightStepPressed && !quickstepManager.enabled)
		{
			if (Player._horizontalSpeedMagnitude > 10f)
			{

				quickstepManager.initialEvents(true);
				quickstepManager.enabled = true;
			}
		}

		else if (Actions.LeftStepPressed && !quickstepManager.enabled)
		{
			if (Player._horizontalSpeedMagnitude > 10f)
			{
				quickstepManager.initialEvents(false);
				quickstepManager.enabled = true;
			}
		}


		//The actions the player can take while the air		
		if (!Player._isGrounded && !coyoteInEffect)
		{
			//Do a homing attack
			if (Actions.Action02Control._HasTarget && Actions.HomingPressed && Actions.Action02.HomingAvailable)
			{

				//Do a homing attack
				if (Actions.Action02 != null && Player._homingDelay_ <= 0)
				{
					if (Actions.Action02Control._isHomingAvailable)
					{
						sounds.HomingAttackSound();
						Actions.ChangeAction(S_Enums.PlayerStates.Homing);
						Actions.Action02.InitialEvents();
					}
				}
			}
			//Do an air dash;
			else if (Actions.Action02.HomingAvailable && Actions.SpecialPressed)
			{
				if (!Actions.Action02Control._HasTarget && _CanDashDuringFall_)
				{
					sounds.AirDashSound();
					Actions.ChangeAction(S_Enums.PlayerStates.JumpDash);
					Actions.Action11.InitialEvents();
				}
			}

			//Do a Double Jump
			else if (Actions.JumpPressed && Actions.Action01._canDoubleJump_)
			{

				Actions.Action01.jumpCount = 0;
				Actions.Action01.InitialEvents(Vector3.up);
				Actions.ChangeAction(S_Enums.PlayerStates.Jump);
			}


			//Do a Bounce Attack
			if (Actions.BouncePressed && Player._RB.velocity.y < 35f)
			{
				Actions.Action06.InitialEvents();
				Actions.ChangeAction(S_Enums.PlayerStates.Bounce);
				//Actions.Action06.ShouldStomp = false;

			}

			//Do a DropDash Attack
			if (Actions.Action08 != null)
			{

				if (!Player._isGrounded && Actions.RollPressed)
				{
					Actions.Action08.TryDropCharge();
				}

				if (Player._isGrounded && Actions.Action08.DropEffect.isPlaying)
				{
					Actions.Action08.DropEffect.Stop();
				}
			}
		}
	}

	IEnumerator CoyoteTime()
    {
		inCoyote = true;
		coyoteInEffect = true;
		float waitFor = _CoyoteTimeBySpeed_.Evaluate(Player._horizontalSpeedMagnitude / 100);

		yield return new WaitForSeconds(waitFor);
		
		coyoteInEffect = false;
    }

	public void cancelCoyote()
    {
		inCoyote = false;
		coyoteInEffect = false;
		StopCoroutine(CoyoteTime());
	}

	private void AssignStats()
    {
		_CoyoteTimeBySpeed_ = Tools.Stats.JumpStats.CoyoteTimeBySpeed;
		_SpeedToStopAt_ = Tools.Stats.SkiddingStats.speedToStopAt;
		_MaximumSlope_ = Tools.Stats.SpinChargeStats.maximumSlopePerformedAt;
		_MaximumSpeed_ = Tools.Stats.SpinChargeStats.maximumSpeedPerformedAt;
		_SkiddingStartPoint_ = Tools.Stats.SkiddingStats.skiddingStartPoint;
		_CanDashDuringFall_ = Tools.Stats.HomingStats.canDashWhenFalling;
		_rollingCapsule_ = Tools.crouchCapsule;
    }

	private void AssignTools()
	{
        Player = GetComponent<S_PlayerPhysics>();
        Input = GetComponent<S_PlayerInput>();
        Actions = GetComponent<S_ActionManager>();
        JumpAction = GetComponent<S_Action01_Jump>();
        Cam = GetComponent<S_Handler_Camera>();
        quickstepManager = GetComponent<S_Handler_quickstep>();
        sounds = Tools.SoundControl;
        quickstepManager.enabled = false;
        characterCapsule = Tools.characterCapsule;
        CharacterAnimator = Tools.CharacterAnimator;
    }

}
