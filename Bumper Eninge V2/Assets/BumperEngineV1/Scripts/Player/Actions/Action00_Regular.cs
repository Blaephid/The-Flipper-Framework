using UnityEngine;
using System.Collections;

public class Action00_Regular : MonoBehaviour {

    public Animator CharacterAnimator;
	CharacterTools Tools;
    PlayerBhysics Player;
	PlayerBinput Input;
    ActionManager Actions;
	CameraControl Cam;
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

    public bool hasSked;
	[HideInInspector] bool CanDashDuringFall;

	[HideInInspector] public bool QuickStepping;
	bool canStep;
	float quickStepSpeed;
	float airStepSpeed;
	float StepCounter = 0f;
	float StepDistance = 10f;
	[HideInInspector] public float airStepDistance = 10f;
	[HideInInspector] public bool StepRight;
	[HideInInspector] public float DistanceToStep;
	RaycastHit hit;
	LayerMask StepPlayermask;
	

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
    }

    private void Start()
    {
		AssignStats();
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
		if((Player.b_normalSpeed < -SkiddingStartPoint) && Player.Grounded)
		{

			if (Player.SpeedMagnitude >= -SkiddingIntensity) Player.AddVelocity(Player.rb.velocity.normalized * SkiddingIntensity * (Player.isRolling ? 0.5f : 1));
			
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
			if (Actions.Action02 != null) {
			Actions.Action02.HomingAvailable = true;
			}

			if (Actions.Action06.BounceCount > 0) {
				Actions.Action06.BounceCount = 0;
			}
				
		}

		if (QuickStepping)
        {

			if (DistanceToStep > 0)
			{
				float stepSpeed;
				if (Player.Grounded)
					stepSpeed = quickStepSpeed;
				else
					stepSpeed = airStepSpeed; 

				//Debug.Log(stepSpeed);

				if (StepRight)
				{
					Vector3 positionTo = transform.position + (CharacterAnimator.transform.right * DistanceToStep);
					float ToTravel = stepSpeed * Time.deltaTime;

					if (DistanceToStep - ToTravel <= 0)
					{
						ToTravel = DistanceToStep;
						StepCounter = 0.4f;
					}

					DistanceToStep -= ToTravel;

					if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, 1.5f, StepPlayermask) && canStep)
						if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, .8f, StepPlayermask))
							transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
					else
						canStep = false;
				}

				// !(Physics.Raycast(transform.position, CharacterAnimator.transform.right * -1, out hit, 4f, StepPlayermask)
				else if (!StepRight)
				{
					Vector3 positionTo = transform.position + (-CharacterAnimator.transform.right * DistanceToStep);
					float ToTravel = stepSpeed * Time.deltaTime;

					if (DistanceToStep - ToTravel <= 0)
					{
						ToTravel = DistanceToStep;
						StepCounter = 0.3f;
					}

					DistanceToStep -= ToTravel;

					if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, 1.5f, StepPlayermask) && canStep)
						if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, .8f, StepPlayermask))
							transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
					else
						canStep = false;
				}
				
			}

			else
			{
				StepCounter -= Time.deltaTime;
				if (StepCounter <= 0)
				{
					QuickStepping = false;
				}
			}


		}

    }

    void Update()
    {
		//Jump
		if (Actions.JumpPressed && Player.Grounded)
		{
			if (Player.SpeedMagnitude < 90)
				Player.rb.velocity *= 0.9f;
			else
				Player.rb.velocity *= 0.85f;

			JumpAction.InitialEvents(Player.GroundNormal);
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
		if (Actions.RollPressed && Player.Grounded && Player.GroundNormal.y > MaximumSlope && Player.HorizontalSpeedMagnitude < MaximumSpeed) 
		{ 
			Actions.ChangeAction(3);
			Actions.Action03.InitialEvents(); 
		}

		//Play Rolling Sound
		else if (Actions.RollPressed && Player.Grounded && (Player.rb.velocity.sqrMagnitude > Player.RollingStartSpeed)) 
		{
			if (!Rolling)
			{

				sounds.SpinningSound();
				rollingCapsule.SetActive(true);
				characterCapsule.SetActive(false);
				rollCounter = 0f;
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
				rollCounter = 0.6f;
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
				Quaternion CharRot = Quaternion.LookRotation(VelocityMod, -Player.Gravity.normalized);
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
		if (Actions.RightStepPressed && !QuickStepping)
        {
			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = false;

			if (Player.HorizontalSpeedMagnitude > 40f)
            {
				QuickStepping = true;
				canStep = true;
				StepRight = true;
				if (Player.Grounded)
					DistanceToStep = StepDistance;
				else
					DistanceToStep = airStepDistance;

			}
		}

		else if (Actions.LeftStepPressed && !QuickStepping)
		{
			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = false;

			if (Player.HorizontalSpeedMagnitude > 40f)
			{
				QuickStepping = true;
				canStep = true;
				StepRight = false;
				if (Player.Grounded)
					DistanceToStep = StepDistance;
				else
					DistanceToStep = airStepDistance;
			}
		}


		//The actions the player can take while the air		
		if (!Player.Grounded)
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

		//Do a LightDash Attack
		if (Actions.InteractPressed && Actions.Action07Control.HasTarget)
		{
			Debug.Log("LightDash");
			Actions.CamResetPressed = false;
			Actions.ChangeAction (7);
			Actions.Action07.InitialEvents ();
		}

		//Do a Spin Kick
		else if (Actions.SpecialPressed && Player.Grounded)
		{
			Actions.Action09.InitialEvents();
			Actions.ChangeAction(9);
		}

	}

	private void AssignStats()
    {
		SpeedToStopAt = Tools.stats.SpeedToStopAt;
		MaximumSlope = Tools.stats.MaximumSlope;
		MaximumSpeed = Tools.stats.MaximumSpeed;
		SkiddingIntensity = Tools.stats.SkiddingIntensity;
		SkiddingStartPoint = Tools.stats.SkiddingStartPoint;
		CanDashDuringFall = Tools.stats.CanDashDuringFall;

		quickStepSpeed = Tools.stats.StepSpeed;
		airStepSpeed = Tools.stats.AirStepSpeed;
		StepDistance = Tools.stats.StepDistance;
		airStepDistance = Tools.stats.AirStepDistance;
		StepPlayermask = Tools.stats.StepLayerMask;
    }

}
