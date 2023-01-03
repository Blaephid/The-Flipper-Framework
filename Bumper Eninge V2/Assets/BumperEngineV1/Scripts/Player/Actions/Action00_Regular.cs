using UnityEngine;
using System.Collections;

public class Action00_Regular : MonoBehaviour {

    public Animator CharacterAnimator;
	CharacterTools Tools;
    PlayerBhysics Player;
	PlayerBinput Input;
    ActionManager Actions;
	CameraControl Cam;
	quickstepHandler quickstepManager;
    public SonicSoundsControl sounds;
	public GameObject characterCapsule;
	public GameObject rollingCapsule;

    public float skinRotationSpeed;
    Action01_Jump JumpAction;
    Quaternion CharRot;

	[HideInInspector] public float MaximumSpeed; //The max amount of speed you can be at to perform a Spin Dash
	[HideInInspector] public float MaximumSlope; //The highest slope you can be on to Spin Dash

	[HideInInspector] public float SpeedToStopAt;

	[HideInInspector] public float SkiddingStartPoint;
	[HideInInspector] public float SkiddingIntensity;
	float AirSkiddingIntensity;

    public bool hasSked;
	[HideInInspector] bool CanDashDuringFall;

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
        Player = GetComponent<PlayerBhysics>();
		Input = GetComponent<PlayerBinput>();
        Actions = GetComponent<ActionManager>();
        JumpAction = GetComponent<Action01_Jump>();
		Cam = GetComponent<CameraControl>();
		Tools = GetComponent<CharacterTools>();
		quickstepManager = GetComponent<quickstepHandler>();
		quickstepManager.enabled = false;
    }

    private void Start()
    {
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
			hasSked = false;

		}

        //Skidding
		if(Player.b_normalSpeed < -SkiddingStartPoint)
		{
			float thisSkid = 0f;
			if (Player.Grounded)
				thisSkid = SkiddingIntensity;
			else
				thisSkid = AirSkiddingIntensity;

			if (Player.SpeedMagnitude >= -thisSkid) Player.AddVelocity(Player.rb.velocity.normalized * thisSkid * (Player.isRolling ? 0.5f : 1));
			
			if (!hasSked && Player.Grounded && !Player.isRolling)
			{
				sounds.SkiddingSound();
				hasSked = true;
				

			}
			if(Player.SpeedMagnitude < 4)
			{
				Player.isRolling = false;
				Player.b_normalSpeed = 0;
				hasSked = false;
				
			}
		}
		else
		{
			hasSked = false;
			
		}

        //Set Homing attack to true
		if (Player.Grounded) 
		{
			inCoyote = false;
			coyoteInEffect = true;
			coyoteRememberDir = Player.GroundNormal;
			coyoteRememberSpeed = Player.rb.velocity.y;

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

    void Update()
    {
		//Jump
		if (Actions.JumpPressed && (Player.Grounded || coyoteInEffect))
		{
			
			if(Player.Grounded)
				JumpAction.InitialEvents(Player.GroundNormal, true, Player.rb.velocity.y);
			else
				JumpAction.InitialEvents(coyoteRememberDir, true, coyoteRememberSpeed);

			Actions.ChangeAction(1);
		}
		

        //Set Animator Parameters
        if (Player.Grounded) { CharacterAnimator.SetInteger("Action", 0); }
        CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
		CharacterAnimator.SetFloat("XZSpeed", Mathf.Abs((Player.rb.velocity.x+Player.rb.velocity.z)/2));
        CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);
		CharacterAnimator.SetFloat("HorizontalInput", Actions.moveX *Player.rb.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);
        CharacterAnimator.SetFloat("NormalSpeed", Player.b_normalSpeed + SkiddingStartPoint);

		//Set Camera to back
		if (Actions.CamResetPressed) 
		{
			if ( Actions.moveX == 0 && Actions.moveY == 0 && Player.b_normalSpeed < 5f)
				Cam.Cam.FollowDirection(6, 14f, -10, 0);
		}

        //Check if rolling
        if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        CharacterAnimator.SetBool("isRolling", Player.isRolling);

		//Do Spindash
		if (Actions.spinChargePressed && Player.Grounded && Player.GroundNormal.y > MaximumSlope && Player.HorizontalSpeedMagnitude < MaximumSpeed) 
		{ 
			Actions.ChangeAction(3);
			Actions.Action03.InitialEvents(); 
		}

		//Play Rolling Sound
		else if (Actions.RollPressed && Player.Grounded && (Player.rb.velocity.sqrMagnitude > Player.RollingStartSpeed)) 
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

        else if (!Actions.RollPressed && rollCounter > minRollTime)
        {
			if (Rolling)
            {
				characterCapsule.SetActive(true);
				rollingCapsule.SetActive(false);
				Rolling = false;
				rollCounter = 0f;
			}
			Player.isRolling = false;

		}

		if(Rolling)
			rollCounter += Time.deltaTime;

		//Set Character Animations and position1
		CharacterAnimator.transform.parent = null;
        
        //Set Skin Rotation
        if (Player.Grounded)
		{ 
			Vector3 newForward = Player.rb.velocity - transform.up * Vector3.Dot(Player.rb.velocity, transform.up);

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
            Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
			if(VelocityMod != Vector3.zero)
            {
				Quaternion CharRot = Quaternion.LookRotation(VelocityMod, -Player.fallGravity.normalized);
				CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
			}
   
        }

		/////Quickstepping
		///
		//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
		if (Actions.RightStepPressed)
        {
			Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
			bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
			if (Facing)
            {
				Actions.RightStepPressed = false;
				Actions.LeftStepPressed = true;
            }
		}
		else if (Actions.LeftStepPressed)
		{
			Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
			bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
			if (Facing)
			{
				Actions.RightStepPressed = true;
				Actions.LeftStepPressed = false;
			}
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
			if (Actions.Action02.HomingAvailable && Actions.Action02Control.HasTarget && Actions.HomingPressed)
			{

				//Do a homing attack
				if (Actions.Action02 != null && Player.HomingDelay <= 0)
				{
					if (Actions.Action02Control.HomingAvailable)
					{
						sounds.HomingAttackSound();
						Actions.ChangeAction(2);
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
					Actions.ChangeAction(11);
					Actions.Action11.InitialEvents();
				}
			}

			//Do a Double Jump
			else if (Actions.JumpPressed && Actions.Action01.canDoubleJump)
			{
				
				//Debug.Log("Do a double jump");
				Actions.Action01.jumpCount = 0;
				Actions.Action01.InitialEvents(Vector3.up);
				Actions.ChangeAction(1);
			}


			//Do a Bounce Attack
			if (Actions.BouncePressed && Player.rb.velocity.y < 35f)
			{
				Actions.Action06.InitialEvents();
				Actions.ChangeAction(6);
				//Actions.Action06.ShouldStomp = false;

			}

			//Do a DropDash Attack
			if (Actions.Action08 != null)
			{

				if (!Player.Grounded && Actions.RollPressed && Actions.Action08 != null && Player.rb.velocity.y < 20f)
				{
					//Actions.Action08.DropDashAvailable = false;
					Actions.ChangeAction(8);
					Actions.Action08.InitialEvents();
				}

				if (Player.Grounded && Actions.Action08.DropEffect.isPlaying)
				{
					Actions.Action08.DropEffect.Stop();
				}
			}
		}

		

		//Do a Spin Kick
		else if (Actions.SpecialPressed && Player.Grounded)
		{
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
		SkiddingIntensity = Tools.stats.SkiddingIntensity;
		AirSkiddingIntensity = Tools.stats.AirSkiddingForce;
		SkiddingStartPoint = Tools.stats.SkiddingStartPoint;
		CanDashDuringFall = Tools.coreStats.CanDashDuringFall;

    }

}
