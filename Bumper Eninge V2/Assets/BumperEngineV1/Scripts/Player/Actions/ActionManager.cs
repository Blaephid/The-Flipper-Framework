using UnityEngine;
using System.Collections;
using SplineMesh;
using UnityEngine.InputSystem;

public class ActionManager : MonoBehaviour {

    public bool trackingEvents;
    public levelEventHandler eventMan;


    public int Action;
	public int PreviousAction { get; set; }

    //Action Scrips, Always leave them in the correct order;
    [Header("Actions")]

    public Action00_Regular Action00;
    public Action01_Jump Action01;
    public Action02_Homing Action02;
    public Action03_SpinCharge Action03;
    public HomingAttackControl Action02Control;
    public Action04_Hurt Action04;
    public HurtControl Action04Control;
    public Action05_Rail Action05;
	public Action06_Bounce Action06;
	public Action07_RingRoad Action07;
	public LightDashControl Action07Control;
	public Action08_DropDash Action08;
    public MoveAlongPath Action10;
    public Action11_JumpDash Action11;
    public Action12_WallRunning Action12;


    [HideInInspector] public bool lockBounce;
    [HideInInspector] public bool lockHoming;
    [HideInInspector] public bool lockJumpDash;
    [HideInInspector] public bool lockDoubleJump;

    //NewInput system
    public PlayerNewInput newInput;

    //NewInput inputs stored
    [HideInInspector] public float moveX;
    [HideInInspector] public float moveY;

    Vector2 CurrentCamMovement;
    [HideInInspector] public float moveCamX;
    [HideInInspector] public float moveCamY;
    float camSensi;
    public float mouseSensi;

    [HideInInspector] public bool JumpPressed;
    [HideInInspector] public bool RollPressed;
    [HideInInspector] public bool SpecialPressed;
    [HideInInspector] public bool LeftStepPressed;
    [HideInInspector] public bool RightStepPressed;
    [HideInInspector] public bool SkidPressed;
    [HideInInspector] public bool BouncePressed;
    [HideInInspector] public bool InteractPressed;
    [HideInInspector] public bool CamResetPressed;
    [HideInInspector] public bool HomingPressed;
    [HideInInspector] public bool spinChargePressed;
    [HideInInspector] public bool killBindPressed;

    [HideInInspector] public bool isPaused;
    [HideInInspector] public bool usingMouse;

    //Etc

    PlayerBhysics Phys;
    CameraControl Cam;
    PlayerBinput Input;
    CharacterTools Tools;

    void Start()
    {
        ChangeAction(0);
    }

    void Awake()
    {
        eventMan = FindObjectOfType<levelEventHandler>();


        if (Phys == null)
        {
            Phys = GetComponent<PlayerBhysics>();
            Input = GetComponent<PlayerBinput>();
            Cam = GetComponent<CameraControl>();
            Tools = GetComponent<CharacterTools>();
        }

        //Managing Inputs

        mouseSensi = Tools.camStats.InputMouseSensi;
        camSensi = Tools.camStats.InputSensi;


    }

    public void MoveInput(InputAction.CallbackContext ctx)
    {
        Vector2 CurrentMovement = ctx.ReadValue<Vector2>();
        moveX = CurrentMovement.x;
        moveY = CurrentMovement.y;
    }

    public void CamInput(InputAction.CallbackContext ctx)
    {
        //Debug.Log("No mouse input");
        usingMouse = false;
        CurrentCamMovement = ctx.ReadValue<Vector2>();
        moveCamX = CurrentCamMovement.x * camSensi;
        moveCamY = CurrentCamMovement.y * camSensi;
    }

    public void CamMouseInput(InputAction.CallbackContext ctx)
    {
        usingMouse = true;
        //Debug.Log("Use Mouse");
        CurrentCamMovement = ctx.ReadValue<Vector2>();
        moveCamX = CurrentCamMovement.x * mouseSensi;
        
        moveCamY = CurrentCamMovement.y * mouseSensi;
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            JumpPressed = ctx.ReadValueAsButton();
        }
    }

    public void Roll(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            RollPressed = ctx.ReadValueAsButton();
        }
    }

    public void LeftStep(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            LeftStepPressed = ctx.ReadValueAsButton();
        }
    }

    public void RightStep(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            RightStepPressed = ctx.ReadValueAsButton();
        }
    }

    public void Special(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            SpecialPressed = ctx.ReadValueAsButton();
        }
    }

    public void Homing(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            HomingPressed = ctx.ReadValueAsButton();
        }
    }

    public void Interact(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            InteractPressed = ctx.ReadValueAsButton();
        }
    }

    public void Power(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!Phys.Grounded)
            {
                BouncePressed = ctx.ReadValueAsButton();
            }
        }

        else if(ctx.canceled)
        {
            BouncePressed = ctx.ReadValueAsButton();
        }
    }

    public void Skid(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            SkidPressed = ctx.ReadValueAsButton();

        }
        else if (ctx.canceled)
        {
            SkidPressed = ctx.ReadValueAsButton();
        }
    }

    public void spinCharge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if(Phys.Grounded)
                spinChargePressed = ctx.ReadValueAsButton();

        }
        else if (ctx.canceled)
        {
            spinChargePressed = ctx.ReadValueAsButton();
        }
    }

    public void killBind(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            killBindPressed = ctx.ReadValueAsButton();

        }
        else if (ctx.canceled)
        {
            killBindPressed = ctx.ReadValueAsButton();
        }
    }

    public void CamReset(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            CamResetPressed = !CamResetPressed;
            if (Cam.Cam.LockCamAtHighSpeed != 20f)
                Cam.Cam.LockCamAtHighSpeed = 20f;
            else
                Cam.Cam.LockCamAtHighSpeed = Cam.Cam.StartLockCam;
        }

    }

        //Additional input help
        //private void Update()
        //{
        //    moveCamX = CurrentCamMovement.x;
        //    moveCamY = CurrentCamMovement.y;
        //}


    public void DeactivateAllActions()
    {
        //Put all actions here
        //Also put variables that you want re-set out of actions
		if (Action00 != null) {
			Action00.enabled = false;
		}
		if (Action01 != null) {
			Action01.enabled = false;
		}
		if (Action02 != null) {
			Action02.enabled = false;
			Action02.ResetHomingVariables();
		}
		if (Action03 != null) {
			Action03.enabled = false;
			Action03.ResetSpinDashVariables();
		}
		if (Action04 != null) {
			Action04.enabled = false;
		}
		if (Action05 != null) {
			Action05.enabled = false;
		}
		if (Action06 != null) {
			Action06.enabled = false;
		}
		if (Action07 != null) {
			Action07.enabled = false;
		}
		if (Action08 != null) {
			Action08.enabled = false;
		}
        if (Action10 != null)
        {
            Action10.enabled = false;
        }
        if (Action11 != null)
        {
            Action11.enabled = false;
        }
        if(Action12 != null)
        {
            Action12.enabled = false;
        }

    }

    //Call this function to change the action
    public void ChangeAction(int ActionToChange)
    {

        //Put an case for all your actions here
        switch (ActionToChange)
        {
            case -1:
                changePossible(ActionToChange);
                break;
            case 0:
                changePossible(ActionToChange);
                Action00.enabled = true;
                break;
            case 1:
                if(!lockDoubleJump)
                {
                    changePossible(ActionToChange);
                    Action01.enabled = true;
                }
                break;
            case 2:
                if (!lockHoming)
                {
                    if (eventMan != null) eventMan.homingAttacksPerformed += 1;
                    changePossible(ActionToChange);
                    Action02.enabled = true;
                }
                break;
            case 3:
                changePossible(ActionToChange);
                Action03.enabled = true;
                break;
            case 4:
                changePossible(ActionToChange);
                Action04.enabled = true;
                break;
            case 5:
                if (eventMan != null) eventMan.RailsGrinded += 1;
                changePossible(ActionToChange);
                Action05.enabled = true;
				break;
			case 6:
                if(!lockBounce)
                {
                    changePossible(ActionToChange);
                    if (eventMan != null) eventMan.BouncesPerformed += 1;
                    Action06.enabled = true;
                }
                break;
			case 7:
                if (eventMan != null) eventMan.ringRoadsPerformed += 1;
                changePossible(ActionToChange);
                Action07.enabled = true;
				break;
			case 8:
                changePossible(ActionToChange);
                Action08.enabled = true;
                break;
            case 10:
                changePossible(ActionToChange);
                Action10.enabled = true;
                break;
            case 11:
                if (!lockJumpDash)
                {
                    if (eventMan != null) eventMan.jumpDashesPerformed += 1;
                    changePossible(ActionToChange);
                    Action11.enabled = true;
                }
                break;
            case 12:
                changePossible(ActionToChange);
                Action12.enabled = true;
                break;

        }

    }

    private void changePossible(int newAction)
    {
        PreviousAction = Action;
        if (PreviousAction == 2)
        {
            Phys.GravityAffects = true;
            actionEnable();
        }

        Action= newAction;
        DeactivateAllActions();
    }

    private void OnEnable()
    {
        //newInput.CameraControl.Enable();
        actionEnable();
    }

    private void OnDisable()
    {
        actionDisable();
    }


    public void actionEnable()
    {
        //newInput.CharacterActions.Enable();

        
    }

    public void actionDisable()
    {
        
        //newInput.CharacterActions.Disable();
 

    }

    public IEnumerator lockAirMoves(float time)
    {
        lockBounce = true;
        lockJumpDash = true;
        lockHoming = true;
        lockDoubleJump = true;

        for(int s = 0; s < time; s++)
        {
            yield return new WaitForFixedUpdate();
            if (Phys.Grounded)
                break;
        }

        lockBounce = false;
        lockJumpDash = false;
        lockHoming = false;
        lockDoubleJump = false;

    }

    public IEnumerator lockBounceOnly(float time)
    {
        lockBounce = true;
        
        for (int v = 0; v < time; v++)
        {
            yield return new WaitForFixedUpdate();
            if (Phys.Grounded && v > 5)
                break;
        }
      
        lockBounce = false;
    }

}
