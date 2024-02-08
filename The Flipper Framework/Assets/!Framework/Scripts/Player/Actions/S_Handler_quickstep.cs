using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_quickstep : MonoBehaviour
{
        S_PlayerPhysics Player;
        S_CharacterTools Tools;
        S_ActionManager Actions;
        S_Handler_Camera Cam;
        S_Enums.PlayerStates startAction;

        Animator CharacterAnimator;

        float _DistanceToStep_;
        float _quickStepSpeed_;
        LayerMask _StepPlayermask_;
        RaycastHit hit;

        bool StepRight;
        float StepCounter;
        bool canStep;
        bool air;

        float timeTrack;

        private void Awake()
        {
                Player = GetComponent<S_PlayerPhysics>();
                Tools = GetComponent<S_CharacterTools>();
                Actions = GetComponent<S_ActionManager>();
                CharacterAnimator = Tools.CharacterAnimator;
                Cam = Tools.GetComponent<S_Handler_Camera>();

                _StepPlayermask_ = Tools.Stats.QuickstepStats.StepLayerMask;

                this.enabled = false;
        }

        public void pressRight()
        {
                Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
                bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
                if (Facing)
                {
                        Actions.RightStepPressed = false;
                        Actions.LeftStepPressed = true;
                }
        }

        public void pressLeft()
        {
                Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
                bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
                if (Facing)
                {
                        Actions.RightStepPressed = true;
                        Actions.LeftStepPressed = false;
                }
        }

        public void initialEvents(bool right)
        {
                startAction = Actions.whatAction;

                if (Actions.eventMan != null) Actions.eventMan.quickstepsPerformed += 1;

                timeTrack = 0;

                if (right)
                {

                        Actions.RightStepPressed = false;
                        Actions.LeftStepPressed = false;


                        canStep = true;
                        StepRight = true;
                        setSpeedAndDistance();

                }
                else
                {
                        Actions.RightStepPressed = false;
                        Actions.LeftStepPressed = false;


                        canStep = true;
                        StepRight = false;
                        setSpeedAndDistance();
                }

        }

        private void setSpeedAndDistance()
        {
                if (Player.Grounded)
                {
                        _quickStepSpeed_ = Tools.Stats.QuickstepStats.stepSpeed;
                        _DistanceToStep_ = Tools.Stats.QuickstepStats.stepDistance;
                        air = false;
                }
                else
                {
                        _DistanceToStep_ = Tools.Stats.QuickstepStats.airStepDistance;
                        _quickStepSpeed_ = Tools.Stats.QuickstepStats.airStepSpeed;
                        air = true;
                }
        }

        // Update is called once per frame
        void FixedUpdate()
        {

                timeTrack = Time.fixedDeltaTime;

                if (air && Player.Grounded)
                        this.enabled = false;
                else if (!air && !Player.Grounded)
                        air = true;

                if (startAction != Actions.whatAction)
                        _DistanceToStep_ = 0;

                if (_DistanceToStep_ > 0)
                {
                        //Debug.Log(DistanceToStep);

                        float stepSpeed = _quickStepSpeed_;

                        //Debug.Log(stepSpeed);

                        if (StepRight)
                        {
                                Vector3 positionTo = transform.position + (CharacterAnimator.transform.right * _DistanceToStep_);
                                float ToTravel = stepSpeed * Time.deltaTime;

                                if (_DistanceToStep_ - ToTravel <= 0)
                                {
                                        ToTravel = _DistanceToStep_;
                                        _DistanceToStep_ = 0;
                                }

                                _DistanceToStep_ -= ToTravel;

                                if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, 1.5f, _StepPlayermask_) && canStep)
                                        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, .8f, _StepPlayermask_))
                                                transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
                                        else
                                                canStep = false;
                        }

                        // !(Physics.Raycast(transform.position, CharacterAnimator.transform.right * -1, out hit, 4f, StepPlayermask)
                        else if (!StepRight)
                        {
                                Vector3 positionTo = transform.position + (-CharacterAnimator.transform.right * _DistanceToStep_);
                                float ToTravel = stepSpeed * Time.deltaTime;

                                if (_DistanceToStep_ - ToTravel <= 0)
                                {
                                        ToTravel = _DistanceToStep_;
                                        _DistanceToStep_ = 0;
                                }

                                _DistanceToStep_ -= ToTravel;

                                if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, 1.5f, _StepPlayermask_) && canStep)
                                        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, .8f, _StepPlayermask_))
                                                transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
                                        else
                                                canStep = false;
                        }

                }

                else
                {
                        StartCoroutine(CoolDown());

                }

        }

        IEnumerator CoolDown()
        {
                if (Player.Grounded)
                        yield return new WaitForSeconds(0.05f);
                else
                        yield return new WaitForSeconds(0.20f);

                this.enabled = false;
        }
}
