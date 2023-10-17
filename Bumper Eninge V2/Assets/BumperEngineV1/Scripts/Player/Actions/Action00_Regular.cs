using UnityEngine;
using System.Collections;

public class Action00_Regular : MonoBehaviour {

    private Animator CharacterAnimator;
	CharacterTools Tools;
    PlayerBhysics Player;
	PlayerBinput Input;
    ActionManager Actions;
	CameraControl Cam;
	quickstepHandler quickstepManager;
    SonicSoundsControl sounds;
	GameObject characterCapsule;
	GameObject rollingCapsule;

    public float skinRotationSpeed;
    Action01_Jump JumpAction;
    Quaternion CharRot;

	float MaximumSpeed; //The max amount of speed you can be at to perform a Spin Dash
	float MaximumSlope; //The highest slope you can be on to Spin Dash

	float SpeedToStopAt;

	float SkiddingStartPoint;


	bool CanDashDuringFall;

	RaycastHit hit;

	AnimationCurve CoyoteTimeBySpeed;
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
		Tools = GetComponent<CharacterTools>();
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

		if(Player.SpeedMagnitude < 15 && Player.MoveInput == Vector3.zero && Player.Grounded)
		{
			Player.b_normalSpeed = 0;
			Player.rb.velocity *= 0.90f;
			Actions.skid.hasSked = false;

		}

		//Skidding

		if (Player.Grounded)
			Actions.skid.RegularSkid();
		else
			Actions.skid.jumpSkid();

		//Set Homing attack to true
		if (Player.Grounded) 
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
		if (Player.Grounded)
			coyoteRememberDir = Player.GroundNormal;
		else
			coyoteRememberDir = transform.up;
		coyoteRememberSpeed = Player.rb.velocity.y;
	}

    void Update()
    {	

        //Set Animator Parameters
        if (Player.Grounded) { CharacterAnimator.SetInteger("Action", 0); }
        CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
		CharacterAnimator.SetFloat("XZSpeed", Mathf.Abs((Player.rb.velocity.x+Player.rb.velocity.z)/2));
        CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);
		CharacterAnimator.SetFloat("HorizontalInput", Actions.moveX *Player.rb.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);
        CharacterAnimator.SetFloat("NormalSpeed", Player.b_normalSpeed + SkiddingStartPoint);

		//Set Character Animations and position1
		CharacterAnimator.transform.parent = null;
        
        //Set Skin Rotation
        if (Player.Grounded)
		{
			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			Vector3 newForward = Player.rb.velocity - transform.up * Vector3.Dot(Player.rb.velocity, transform.up);
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
			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			Vector3 VelocityMod = new Vector3(releVec.x, 0, releVec.z);
			//VelocityMod = Player.rb.velocity;

			Vector3 newForward = Player.rb.velocity - transform.up * Vector3.Dot(Player.rb.velocity, transform.up);
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
			rollingCapsule.SetActive(true);
			characterCapsule.SetActive(false);
			//rollCounter = 0f;
		}
		Player.isRolling = true;
		Rolling = true;
	}

	void unCurl()
    {
		CapsuleCollider col = rollingCapsule.GetComponent<CapsuleCollider>();


		if (!Physics.Raycast(col.transform.position + col.center, CharacterAnimator.transform.up, 4))
		{
			characterCapsule.SetActive(true);
			rollingCapsule.SetActive(false);
			rollCounter = 0f;
			Rolling = false;
			Player.isRolling = false;

		}

	}

	void handleInputs()
    {

		//Jump
		if (Actions.JumpPressed && (Player.Grounded || coyoteInEffect))
		{

			if (Player.Grounded)
				JumpAction.InitialEvents(Player.GroundNormal, true, Player.rb.velocity.y);
			else
				JumpAction.InitialEvents(coyoteRememberDir, true, coyoteRememberSpeed);

			Actions.ChangeAction(ActionManager.States.Jump);
		}

		//Set Camera to back
		if (Actions.CamResetPressed)
		{
			if (Actions.moveVec == Vector2.zero && Player.b_normalSpeed < 5f)
				Cam.Cam.FollowDirection(6, 14f, -10, 0);
		}

		

		//Do Spindash
		if (Actions.spinChargePressed && Player.Grounded && Player.GroundNormal.y > MaximumSlope && Player.HorizontalSpeedMagnitude < MaximumSpeed)
		{
			Actions.ChangeAction(ActionManager.States.SpinCharge);
			Actions.Action03.InitialEvents();
		}

        //Check if rolling
        if (Player.Grounded && Player.isRolling) 
		{ 
			CharacterAnimator.SetInteger("Action", 1); 
		}
        CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Change to rolling state
        if (Actions.RollPressed && Player.Grounded)
		{
			Curl();
		}

		//Exit rolling state
		if ((!Actions.RollPressed && rollCounter > minRollTime) | !Player.Grounded)
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
			if (Player.HorizontalSpeedMagnitude > 10f)
			{

				quickstepManager.initialEvents(true);
				quickstepManager.enabled = true;
			}
		}

		else if (Actions.LeftStepPressed && !quickstepManager.enabled)
		{
			if (Player.HorizontalSpeedMagnitude > 10f)
			{
				quickstepManager.initialEvents(false);
				quickstepManager.enabled = true;
			}
		}


		//The actions the player can take while the air		
		if (!Player.Grounded && !coyoteInEffect)
		{
			//Do a homing attack
			if (Actions.Action02Control.HasTarget && Actions.HomingPressed && Actions.Action02.HomingAvailable)
			{

				//Do a homing attack
				if (Actions.Action02 != null && Player.HomingDelay <= 0)
				{
					if (Actions.Action02Control.HomingAvailable)
					{
						sounds.HomingAttackSound();
						Actions.ChangeAction(ActionManager.States.Homing);
						Actions.Action02.InitialEvents();
					}
				}
			}
			//Do an air dash;
			else if (Actions.Action02.HomingAvailable && Actions.SpecialPressed)
			{
				if (!Actions.Action02Control.HasTarget && CanDashDuringFall)
				{
					sounds.AirDashSound();
					Actions.ChangeAction(ActionManager.States.JumpDash);
					Actions.Action11.InitialEvents();
				}
			}

			//Do a Double Jump
			else if (Actions.JumpPressed && Actions.Action01.canDoubleJump)
			{

				Actions.Action01.jumpCount = 0;
				Actions.Action01.InitialEvents(Vector3.up);
				Actions.ChangeAction(ActionManager.States.Jump);
			}


			//Do a Bounce Attack
			if (Actions.BouncePressed && Player.rb.velocity.y < 35f)
			{
				Actions.Action06.InitialEvents();
				Actions.ChangeAction(ActionManager.States.Bounce);
				//Actions.Action06.ShouldStomp = false;

			}

			//Do a DropDash Attack
			if (Actions.Action08 != null)
			{

				if (!Player.Grounded && Actions.RollPressed)
				{
					Actions.Action08.TryDropCharge();
				}

				if (Player.Grounded && Actions.Action08.DropEffect.isPlaying)
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
		float waitFor = CoyoteTimeBySpeed.Evaluate(Player.HorizontalSpeedMagnitude / 100);

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
		CoyoteTimeBySpeed = Tools.coreStats.CoyoteTimeOverSpeed;
		SpeedToStopAt = Tools.stats.SpeedToStopAt;
		MaximumSlope = Tools.coreStats.MaximumSlope;
		MaximumSpeed = Tools.coreStats.MaximumSpeed;
		SkiddingStartPoint = Tools.stats.SkiddingStartPoint;
		CanDashDuringFall = Tools.coreStats.CanDashDuringFall;
		rollingCapsule = Tools.crouchCapsule;

    }

	private void AssignTools()
	{
        Player = GetComponent<PlayerBhysics>();
        Input = GetComponent<PlayerBinput>();
        Actions = GetComponent<ActionManager>();
        JumpAction = GetComponent<Action01_Jump>();
        Cam = GetComponent<CameraControl>();
        quickstepManager = GetComponent<quickstepHandler>();
        sounds = GetComponent<SonicSoundsControl>();
        quickstepManager.enabled = false;
        characterCapsule = Tools.characterCapsule;
        CharacterAnimator = Tools.CharacterAnimator;
    }

}
