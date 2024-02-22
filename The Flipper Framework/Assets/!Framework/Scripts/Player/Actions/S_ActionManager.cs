using UnityEngine;
using System.Collections;
using SplineMesh;
using UnityEngine.InputSystem;

public class S_ActionManager : MonoBehaviour
{

        public bool trackingEvents;
        public levelEventHandler eventMan;


        public S_Enums.PlayerStates whatAction;
        public S_Enums.PlayerStates whatPreviousAction { get; set; }

        //Action Scrips, Always leave them in the correct order;
        [Header("Actions")]
        public S_Action00_Regular Action00;
        public S_Action01_Jump Action01;
        public S_Action02_Homing Action02;
        public S_Action03_SpinCharge Action03;
        public S_Handler_HomingAttack Action02Control;
        public S_Action04_Hurt Action04;
        public S_Handler_Hurt Action04Control;
        public S_Action05_Rail Action05;
        public S_Action06_Bounce Action06;
        public S_Action07_RingRoad Action07;
        public S_Handler_RingRoad Action07Control;
        public S_Action08_DropCharge Action08;
        public S_Action10_FollowAutoPath Action10;
        public S_Action11_JumpDash Action11;
        public S_Action12_WallRunning Action12;
        public S_Action13_Hovering Action13;
        public S_Handler_Skidding skid;


        [HideInInspector] public bool lockBounce;
        [HideInInspector] public bool lockHoming;
        [HideInInspector] public bool lockJumpDash;
        [HideInInspector] public bool lockDoubleJump;

        [HideInInspector] public bool isPaused;

        //Etc

        S_PlayerPhysics Phys;

        void Start()
        {
                ChangeAction(S_Enums.PlayerStates.Regular);
        }

        void Awake()
        {
                eventMan = FindObjectOfType<levelEventHandler>();


                if (Phys == null)
                {
                        Phys = GetComponent<S_PlayerPhysics>();
                }


        }


        public void DeactivateAllActions()
        {
                //Put all actions here
                //Also put variables that you want re-set out of actions
                if (Action00 != null)
                {
                        Action00.enabled = false;
                }
                if (Action01 != null)
                {
                        Action01.enabled = false;
                }
                if (Action02 != null)
                {
                        Action02.enabled = false;
                        Action02.ResetHomingVariables();
                }
                if (Action03 != null)
                {
                        Action03.enabled = false;
                        Action03.ResetSpinDashVariables();
                }
                if (Action04 != null)
                {
                        Action04.enabled = false;
                }
                if (Action05 != null)
                {
                        Action05.enabled = false;
                }
                if (Action06 != null)
                {
                        Action06.enabled = false;
                }
                if (Action07 != null)
                {
                        Action07.enabled = false;
                }
                if (Action08 != null)
                {
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
                if (Action12 != null)
                {
                        Action12.enabled = false;
                }
                if (Action13 != null)
                {
                        Action13.enabled = false;
                }

        }

        //Call this function to change the action
        public void ChangeAction(S_Enums.PlayerStates ActionToChange)
        {
                //Put an case for all your actions here
                switch (ActionToChange)
                {
                        case S_Enums.PlayerStates.Regular:
                                changePossible(ActionToChange);
                                Action00.enabled = true;
                                break;
                        case S_Enums.PlayerStates.Jump:
                                if (!lockDoubleJump)
                                {
                                        changePossible(ActionToChange);
                                        Action01.enabled = true;
                                }
                                break;
                        case S_Enums.PlayerStates.Homing:
                                if (!lockHoming)
                                {
                                        if (eventMan != null) eventMan.homingAttacksPerformed += 1;
                                        changePossible(ActionToChange);
                                        Action02.enabled = true;
                                }
                                break;
                        case S_Enums.PlayerStates.JumpDash:
                                if (!lockJumpDash)
                                {
                                        if (eventMan != null) eventMan.jumpDashesPerformed += 1;
                                        changePossible(ActionToChange);
                                        Action11.enabled = true;
                                }
                                break;
                        case S_Enums.PlayerStates.SpinCharge:
                                changePossible(ActionToChange);
                                Action03.enabled = true;
                                break;
                        case S_Enums.PlayerStates.Hurt:
                                changePossible(ActionToChange);
                                Action04.enabled = true;
                                break;
                        case S_Enums.PlayerStates.Rail:
                                if (eventMan != null) eventMan.RailsGrinded += 1;
                                changePossible(ActionToChange);
                                Action05.enabled = true;
                                break;
                        case S_Enums.PlayerStates.Bounce:
                                if (!lockBounce)
                                {
                                        changePossible(ActionToChange);
                                        if (eventMan != null) eventMan.BouncesPerformed += 1;
                                        Action06.enabled = true;
                                }
                                break;
                        case S_Enums.PlayerStates.RingRoad:
                                if (eventMan != null) eventMan.ringRoadsPerformed += 1;
                                changePossible(ActionToChange);
                                Action07.enabled = true;
                                break;
                        case S_Enums.PlayerStates.DropCharge:
                                changePossible(ActionToChange);
                                Action08.enabled = true;
                                break;
                        case S_Enums.PlayerStates.Path:
                                changePossible(ActionToChange);
                                Action10.enabled = true;
                                break;
                        case S_Enums.PlayerStates.WallRunning:
                                changePossible(ActionToChange);
                                Action12.enabled = true;
                                break;
                        case S_Enums.PlayerStates.Hovering:
                                changePossible(ActionToChange);
                                Action13.enabled = true;
                                break;

                }

        }

        private void changePossible(S_Enums.PlayerStates newAction)
        {
                whatPreviousAction = whatAction;

                switch (whatPreviousAction)
                {
                        case S_Enums.PlayerStates.Homing:
                                Phys._isGravityOn = true;
                                actionEnable();
                                break;
                }

                whatAction = newAction;
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

                for (int s = 0; s < time; s++)
                {
                        yield return new WaitForFixedUpdate();
                        if (Phys._isGrounded)
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
                        if (Phys._isGrounded)
                                break;
                }

                lockBounce = false;
        }

        private void FixedUpdate()
        {
                //Debug.Log("Action == " +Action);
        }

}
