using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class S_PlayerPhysics : MonoBehaviour
{
    [HideInInspector] public S_ActionManager Action;
    S_CharacterTools Tools;
    Transform mainSkin;

    static public S_PlayerPhysics MasterPlayer;

    [Header("Movement Values")]

    float _startAcceleration_ = 0.5f;
    float MoveAccell;

    AnimationCurve _accelOverSpeed_;
    float _accelShiftOverSpeed_;
    float _decelShiftOverSpeed_;

    [HideInInspector] public float _moveDeceleration_ = 1.3f;
    AnimationCurve _decelBySpeed_;
    float _airDecell_ = 1.05f;
    float _naturalAirDecel_ = 1.01f;

    float _tangentialDrag_;
    float _tangentialDragShiftSpeed_;

    float _turnSpeed_ = 16f;

    AnimationCurve _turnRateOverAngle_;
    AnimationCurve _turnRateOverSpeed_;
    AnimationCurve _tangDragOverAngle_;
    AnimationCurve _tangDragOverSpeed_;

    float _startTopSpeed_ = 65f;
    [HideInInspector] public float TopSpeed;
    float _startMaxSpeed_ = 230f;
    [HideInInspector] public float MaxSpeed;
    float _startMaxFallingSpeed_ = -500f;
    float MaxFallingSpeed;
    float _startJumpPower_ = 2;
    float m_JumpPower;

    float _groundStickingDistance_ = 1;
    float _groundStickingPower_ = -1;

    float _slopeEffectLimit_ = 0.9f;
    float _standOnSlopeLimit_ = 0.8f;
    float _slopePower_ = 0.5f;
    float _slopeRunningAngleLimit_ = 0.5f;
    AnimationCurve _slopeSpeedLimit_;

    float _generalHillMultiplier_ = 1;
    float _uphillMultiplier_ = 0.5f;
    float _downhillMultiplier_ = 2;
    float _startDownhillMultiplier_ = -7;

    AnimationCurve _slopePowerOverSpeed_;
    AnimationCurve _UpHillByTime_;
    float SlopePowerShiftSpeed;
    float LandingConversionFactor = 2;

    [Header("AirMovementExtras")]
    float _airControlAmmount_ = 2;
    //float AirSkiddingForce = 10;g
    bool _shouldStopAirMovementIfNoInput_ = false;
    float _keepNormalForThis_ = 0.083f;


    public bool Grounded { get; set; }
    public Vector3 GroundNormal { get; set; }
    public Vector3 CollisionPointsNormal { get; set; }

    public Rigidbody rb { get; set; }

    [HideInInspector] public bool GravityAffects = true;
    [HideInInspector] public Vector3 _startFallGravity_;
    [HideInInspector] public Vector3 fallGravity;
    Vector3 _upGravity_;
    public Vector3 _moveInput { get; set; }

    [HideInInspector] public RaycastHit groundHit;
    Transform CollisionPoint;

    S_Control_SoundsPlayer sounds;
    [HideInInspector] public float _homingDelay_;




    [Header("Rolling Values")]

    float _rollingLandingBoost_;
    float _rollingDownhillBoost_;
    float _rollingUphillBoost_;
    float _rollingStartSpeed_;
    float _rollingTurningDecrease_;
    float _rollingFlatDecell_;
    float _slopeTakeoverAmount_; // This is the normalized slope angle that the player has to be in order to register the land as "flat"
    public bool isRolling { get; set; }

    //Cache

    public float curvePosAcell { get; set; }

    float curvePosDecell = 1f;

    float curvePosTurn;
    public float curvePosTang { get; set; }
    public float curvePosSlope { get; set; }
    public float b_normalSpeed { get; set; }
    public Vector3 b_normalVelocity { get; set; }
    public Vector3 b_tangentVelocity { get; set; }
    public Vector3 playerPos { get; set; }

    //Etc

    Vector3 KeepNormal;
    float KeepNormalCounter;
    public bool WasOnAir { get; set; }
    public Vector3 PreviousInput { get; set; }
    public Vector3 RawInput { get; set; }
    public Vector3 PreviousRawInput { get; set; }
    public Vector3 PreviousRawInputForAnim { get; set; }
    public float SpeedMagnitude { get; set; }
    public float HorizontalSpeedMagnitude { get; set; }

    float timeUpHill;

    [Header("Greedy Stick Fix")]
    public bool EnableDebug;
    public float TimeOnGround { get; set; }
    RaycastHit hitSticking, hitRot;
    [HideInInspector] public float RayToGroundDistancecor, RayToGroundRotDistancecor;

    [Tooltip("This is the values of the Lerps when the player encounters a slope , the first one is negative slopes (loops), and the second one is positive Slopes (imagine running on the outside of a loop),This values shouldnt be touched unless yuou want to go absurdly faster. Default values 0.885 and 1.5")]
    Vector2 StickingLerps = new Vector2(0.885f, 1.5f);
    [Tooltip("This is the limit from 0 to 1 the degrees that the player should be sticking 0 is no angle , 1 is everything bellow 90°, and 0.5 is 45° angles, default 0.4")]
    float StickingNormalLimit = 0.4f;
    [Tooltip("This is the cast ahead when the player hits a slope, this will be used to predict it's path if it is going on a high speed. too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, default value 1.9")]
    float StickCastAhead = 1.9f;
    [Tooltip("This is the position above the raycast hit point that the player will be placed if he is loosing grip on positive G turns, this value will snap the player back into the mesh, it shouldnt be moved unless you scale the collider, default value 0.6115")]
    [HideInInspector] public float negativeGHoverHeight = 0.6115f;
    float RayToGroundDistance = 0.55f;
    float RaytoGroundSpeedRatio = 0.01f;
    float RaytoGroundSpeedMax = 2.4f;
    float RayToGroundRotDistance = 1.1f;
    float RaytoGroundRotSpeedMax = 2.6f;
    float RotationResetThreshold = -0.1f;

    [HideInInspector] public LayerMask _Groundmask_;



    private void Start()
    {
        Tools = GetComponent<S_CharacterTools>();
        AssignTools();
        AssignStats();
        mainSkin = Tools.mainSkin;


    }

    void FixedUpdate()
    {
        //Debug.Log(GroundNormal);
        //Debug.Log(Action.Action);
        //Debug.Log(isRolling);
        //Debug.Log(HorizontalSpeedMagnitude);


        TimeOnGround += Time.deltaTime;
        if (!Grounded) TimeOnGround = 0;

        if(Action.whatAction != S_Enums.PlayerStates.Path)
            GeneralPhysics();

        if (_homingDelay_ > 0)
        {
            _homingDelay_ -= Time.deltaTime;
        }


        //Debug.Log(MoveInput);
    }

    void Update()
    {
        SpeedMagnitude = rb.velocity.magnitude;
  
    
        Vector3 releVec = getRelevantVec(rb.velocity);
        HorizontalSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;
        playerPos = transform.position;
    }

    public Vector3 getRelevantVec(Vector3 vec)
    {
        return transform.InverseTransformDirection(vec);
        //if (!Grounded)
        //{
        //    return transform.InverseTransformDirection(vec);
        //    //Vector3 releVec = transform.InverseTransformDirection(rb.velocity.normalized);
        //}
        //else
        //{
        //    return transform.InverseTransformDirection(vec);
        //    return Vector3.ProjectOnPlane(vec, groundHit.normal);
        //}
    }


    void GeneralPhysics()
    {
        //Set Previous input
        if (RawInput.sqrMagnitude >= 0.03f)
        {
            PreviousRawInputForAnim = RawInput * 90;
            PreviousRawInputForAnim = PreviousRawInputForAnim.normalized;
        }

        if (_moveInput.sqrMagnitude >= 0.9f)
        {
            PreviousInput = _moveInput;
        }
        if (RawInput.sqrMagnitude >= 0.9f)
        {
            PreviousRawInput = RawInput;
        }

        //Set Curve thingies
        curvePosAcell = Mathf.Lerp(curvePosAcell, _accelOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * _accelShiftOverSpeed_);
        curvePosDecell = Mathf.Lerp(curvePosDecell, _decelBySpeed_.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * _decelShiftOverSpeed_);
        curvePosTang = Mathf.Lerp(curvePosTang, _tangDragOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * _tangentialDragShiftSpeed_);
        curvePosSlope = Mathf.Lerp(curvePosSlope, _slopePowerOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * SlopePowerShiftSpeed);

        // Do it for X and Z
        if (HorizontalSpeedMagnitude > MaxSpeed)
        {
            Vector3 ReducedSpeed = rb.velocity;
            float keepY = rb.velocity.y;
            ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, MaxSpeed);
            ReducedSpeed.y = keepY;
            rb.velocity = ReducedSpeed;
        }

        //Do it for Y
        //if (Mathf.Abs(rb.velocity.y) > MaxFallingSpeed)
        //{
        //    Vector3 ReducedSpeed = rb.velocity;
        //    float keepX = rb.velocity.x;
        //    float keepZ = rb.velocity.z;
        //    ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, MaxSpeed);
        //    ReducedSpeed.x = keepX;
        //    ReducedSpeed.z = keepZ;
        //    rb.velocity = ReducedSpeed;
        //}

        //Rotate Colliders     
        if (EnableDebug)
        {
            Debug.DrawRay(transform.position + (transform.up * 2) + transform.right, -transform.up * (2f + RayToGroundRotDistancecor), Color.red);
        }

        alignWithGround();

        CheckForGround();
    }

    public void alignWithGround()
    {
        

        //if ((Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out hitRot, 2f + RayToGroundRotDistancecor, Playermask)))
        if (Grounded)
        {
            GroundNormal = groundHit.normal;

            KeepNormal = GroundNormal;

            transform.rotation = Quaternion.FromToRotation(transform.up, GroundNormal) * transform.rotation;
            //transform.rotation = Quaternion.LookRotation(Vector3.forward, GroundNormal);
            //transform.up = GroundNormal;

            KeepNormalCounter = 0;
                  
        }
        else
        {
            //Keep the rotation after exiting the ground for a while, to avoid collision issues.

            KeepNormalCounter += Time.deltaTime;
            if (KeepNormalCounter < _keepNormalForThis_)
            //if (KeepNormalCounter < 1f)
            {
                transform.rotation = Quaternion.FromToRotation(transform.up, KeepNormal) * transform.rotation;

            }
            else
            {
                //Debug.Log(KeepNormal.y);

                //if (transform.up.y < RotationResetThreshold)
                if (KeepNormal.y < RotationResetThreshold)
                {
                    KeepNormal = Vector3.up;

                    if (mainSkin.right.y >= -mainSkin.right.y)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, mainSkin.right) * transform.rotation, 10f);
                    else
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, -mainSkin.right) * transform.rotation, 10f);

                    if (Vector3.Dot(transform.up, Vector3.up) > 0.99)
                        KeepNormal = Vector3.up;

                }
                else
                {
                    Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 10f);
                }
            }
        }
    }

    Vector3 HandleGroundControl(float deltaTime, Vector3 input)
    {
        if(Action.whatAction != S_Enums.PlayerStates.JumpDash && Action.whatAction != S_Enums.PlayerStates.WallRunning)
        {

            //By Damizean

            // We assume input is already in the Player's local frame...
            // Fetch velocity in the Player's local frame, decompose into lateral and vertical
            // components.

            Vector3 velocity = rb.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);

            Vector3 lateralVelocity = new Vector3(localVelocity.x, 0.0f, localVelocity.z);
            Vector3 verticalVelocity = new Vector3(0.0f, localVelocity.y, 0.0f);

            // If there is some input...

            if (input.sqrMagnitude != 0.0f)
            {

                // Normalize to get input direction.

                

                Vector3 inputDirection = input.normalized;
                float inputMagnitude = input.magnitude;

                // Step 1) Determine angle and rotation between current lateral velocity and desired direction.
                //         Prevent invalid rotations if no lateral velocity component exists.

                float deviationFromInput = Vector3.Angle(lateralVelocity, inputDirection) / 180.0f;
                Quaternion lateralToInput = Mathf.Approximately(lateralVelocity.sqrMagnitude, 0.0f)
                                            ? Quaternion.identity
                                            : Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);

                // Step 2) Let the user retain some component of the velocity if it's trying to move in
                //         nearby directions from the current one. This should improve controlability.
              
                float turnRate = _turnRateOverAngle_.Evaluate(deviationFromInput);
                turnRate *= _turnRateOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed);
                //lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, Mathf.Deg2Rad * TurnSpeed * turnRate * Time.deltaTime, 0.0f);

                lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, _turnSpeed_ * turnRate * Time.deltaTime, 0.0f);
                


                // Step 3) Further lateral velocity into normal (in the input direction) and tangential
                //         components. Note: normalSpeed is the magnitude of normalVelocity, with the added
                //         bonus that it's signed. If positive, the speed goes towards the same
                //         direction than the input :)

                var normalDot = Vector3.Dot(lateralVelocity.normalized, inputDirection.normalized);

                //if (Mathf.Abs(normalDot) <= 0.6f && normalDot > -0.6f)
                //{
                //    inputDirection = Vector3.Slerp(lateralVelocity.normalized, inputDirection, 0.005f);
                //}

                float normalSpeed = Vector3.Dot(lateralVelocity, inputDirection);
                Vector3 normalVelocity = inputDirection * normalSpeed;
                Vector3 tangentVelocity = lateralVelocity - normalVelocity;
                float tangentSpeed = tangentVelocity.magnitude;

                // Step 4) Apply user control in this direction.

                if (normalSpeed < TopSpeed)
                {


                    // Accelerate towards the input direction.
                    normalSpeed += (isRolling ? 0 : MoveAccell) * deltaTime * inputMagnitude;

                    normalSpeed = Mathf.Min(normalSpeed, TopSpeed);

                    // Rebuild back the normal velocity with the correct modulus.

                    normalVelocity = inputDirection * normalSpeed;
                }

                // Step 5) Dampen tangential components.

                float dragRate = _tangDragOverAngle_.Evaluate(deviationFromInput)
                                * _tangDragOverSpeed_.Evaluate((tangentSpeed * tangentSpeed) / (MaxSpeed * MaxSpeed));

                tangentVelocity = Vector3.MoveTowards(tangentVelocity, Vector3.zero, _tangentialDrag_ * dragRate * deltaTime);

                lateralVelocity = normalVelocity + tangentVelocity;

                //Export nescessary variables

                b_normalSpeed = normalSpeed;
                b_normalVelocity = normalVelocity;
                b_tangentVelocity = tangentVelocity;

            }

            // Otherwise, apply some damping as to decelerate Sonic.
            if (Grounded)
            {
                float DecellAmount = 1;
                if (isRolling && GroundNormal.y > _slopeTakeoverAmount_ && HorizontalSpeedMagnitude > 10)
                {
                    DecellAmount = _rollingFlatDecell_ * curvePosDecell;
                    if (input.sqrMagnitude == 0)
                        DecellAmount *= _moveDeceleration_;
                }

                else if (input.sqrMagnitude == 0)
                {
                    DecellAmount = _moveDeceleration_ * curvePosDecell;
                }
                lateralVelocity /= DecellAmount;
            }


            // Compose local velocity back and compute velocity back into the Global frame.

            localVelocity = lateralVelocity + verticalVelocity;

            //new line for the stick to ground from GREEDY


            velocity = transform.TransformDirection(localVelocity);

            if (Grounded)
                velocity = StickToGround(velocity);

            return velocity;
        }
        return rb.velocity;

    }

    void GroundMovement()
    {
        //Stop Rolling
        //if (HorizontalSpeedMagnitude < 3)
        //{
        //    isRolling = false;
        //}

        //Slope Physics
        SlopePlysics();

        // Call Ground Control

       

        Vector3 setVelocity = HandleGroundControl(1, _moveInput * curvePosAcell);
        rb.velocity = setVelocity;


    }

    void SlopePlysics()
    {
        //ApplyLandingSpeed
        if (WasOnAir)
        {
            Vector3 Addsped;

            if (!isRolling)
            {
                Addsped = GroundNormal * LandingConversionFactor;
                //StickToGround(GroundStickingPower);
            }
            else
            {
                Addsped = (GroundNormal * LandingConversionFactor) * _rollingLandingBoost_;
                //StickToGround(GroundStickingPower * RollingLandingBoost);
                sounds.SpinningSound();
            }

            Addsped.y = 0;
            AddVelocity(Addsped);
            WasOnAir = false;
        }

        //Get out of slope if speed is too low
        if (HorizontalSpeedMagnitude < _slopeSpeedLimit_.Evaluate(GroundNormal.y))
        {
            if(_slopeRunningAngleLimit_ > GroundNormal.y)
            {
                //transform.rotation = Quaternion.identity;
                Grounded = false;
                AddVelocity(GroundNormal * 1.5f);
            }

        }
    


        //Apply slope power
        if (GroundNormal.y < _slopeEffectLimit_)
        {

            if (timeUpHill < 0)
                timeUpHill = 0;

            if (rb.velocity.y > _startDownhillMultiplier_)
            {
                timeUpHill += Time.deltaTime;
                //Debug.Log(p_rigidbody.velocity.y);
                if (!isRolling)
                {
                    Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _uphillMultiplier_ * _generalHillMultiplier_, 0);
                    force *= _UpHillByTime_.Evaluate(timeUpHill);
                    AddVelocity(force);
                }
                else
                {
                    Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _uphillMultiplier_ * _generalHillMultiplier_, 0) * _rollingUphillBoost_;
                    force *= _UpHillByTime_.Evaluate(timeUpHill);
                    AddVelocity(force);
                }
            }

            else
            {
                timeUpHill -= Time.deltaTime * 0.8f;
                if (_moveInput != Vector3.zero && b_normalSpeed > 0)
                {
                    if (!isRolling)
                    {
                        Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _downhillMultiplier_ * _generalHillMultiplier_, 0);
                        AddVelocity(force);
                    }
                    else
                    {
                        Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _downhillMultiplier_ * _generalHillMultiplier_, 0) * _rollingDownhillBoost_;
                        AddVelocity(force);
                    }

                }
                else if (GroundNormal.y < _standOnSlopeLimit_)
                {
                    Vector3 force = new Vector3(0, _slopePower_ * curvePosSlope, 0);
                    AddVelocity(force);
                }
            }
        }
        else
            timeUpHill = 0;

    }

    public Vector3 StickToGround(Vector3 Velocity)
    {
        Vector3 result = Velocity;
        if (EnableDebug)
        {
            Debug.Log("Before: " + result + "speed " + result.magnitude);
        }
        if (TimeOnGround > 0.1f && SpeedMagnitude > 1)
        {
            float DirectionDot = Vector3.Dot(rb.velocity.normalized, groundHit.normal);
            Vector3 normal = groundHit.normal;
            Vector3 Raycasterpos = rb.position + (groundHit.normal * -0.12f);

            if (EnableDebug)
            {
                Debug.Log("Speed: " + SpeedMagnitude + "\n Direction DOT: " + DirectionDot + " \n Velocity Normal:" + rb.velocity.normalized + " \n  Ground normal : " + groundHit.normal);
                Debug.DrawRay(groundHit.point + (transform.right * 0.2F), groundHit.normal * 3, Color.yellow, 1);
            }

            //If the Raycast Hits something, it adds it's normal to the ground normal making an inbetween value the interpolates the direction;
            if (Physics.Raycast(Raycasterpos, rb.velocity.normalized, out hitSticking, SpeedMagnitude * StickCastAhead * Time.deltaTime, _Groundmask_))
            {
                if (EnableDebug) Debug.Log("AvoidingGroundCollision");

                if (Vector3.Dot(normal, hitSticking.normal) > 0.15f) //avoid flying off Walls
                {
                    normal = hitSticking.normal.normalized;
                    Vector3 Dir = Align(Velocity, normal.normalized);
                    result = Vector3.Lerp(Velocity, Dir, StickingLerps.x);
                    transform.position = groundHit.point + normal * negativeGHoverHeight;
                    if (EnableDebug)
                    {
                        Debug.DrawRay(groundHit.point, normal * 3, Color.red, 1);
                        Debug.DrawRay(transform.position, Dir.normalized * 3, Color.yellow, 1);
                        Debug.DrawRay(transform.position + transform.right, Dir.normalized * 3, Color.cyan + Color.black, 1);
                    }
                }
            }
            else
            {
                if (Mathf.Abs(DirectionDot) < StickingNormalLimit) //avoid SuperSticking
                {
                    Vector3 Dir = Align(Velocity, normal.normalized);
                    float lerp = StickingLerps.y;
                    if (Physics.Raycast(Raycasterpos + (rb.velocity * StickCastAhead * Time.deltaTime), -groundHit.normal, out hitSticking, 2.5f, _Groundmask_))
                    {
                        float dist = hitSticking.distance;
                        if (EnableDebug)
                        {
                            Debug.Log("PlacedDown" + dist);
                            Debug.DrawRay(Raycasterpos + (rb.velocity * StickCastAhead * Time.deltaTime), -groundHit.normal * 3, Color.cyan, 2);
                        }
                        if (dist > 1.5f)
                        {
                            if (EnableDebug) Debug.Log("ForceDown");
                            lerp = 5;
                            result += (-groundHit.normal * 10);
                            transform.position = groundHit.point + normal * negativeGHoverHeight;
                        }
                    }

                    result = Vector3.LerpUnclamped(Velocity, Dir, lerp);

                    if (EnableDebug)
                    {
                        Debug.Log("Lerp " + lerp + " Result " + result);
                        Debug.DrawRay(groundHit.point, normal * 3, Color.green, 0.6f);
                        Debug.DrawRay(transform.position, result.normalized * 3, Color.grey, 0.6f);
                        Debug.DrawRay(transform.position + transform.right, result.normalized * 3, Color.cyan + Color.black, 0.6f);
                    }
                }

            }

            result += (-groundHit.normal * 2); // traction addition
        }
        if (EnableDebug)
        {
            Debug.Log("After: " + result + "speed " + result.magnitude);
        }
        return result;

    }



    Vector3 Align(Vector3 vector, Vector3 normal)
    {
        //typically used to rotate a movement vector by a surface normal
        Vector3 tangent = Vector3.Cross(normal, vector);
        Vector3 newVector = -Vector3.Cross(normal, tangent);
        vector = newVector.normalized * vector.magnitude;
        return vector;
    }

    public void AddVelocity(Vector3 force)
    {
        rb.velocity += force;
    }

    void AirMovement()
    {
        Vector3 setVelocity;
        //AddSpeed
        //Air Skidding  
        //if (b_normalSpeed < 0 && (Action.Action  == ActionManager.States.Regular || Action.Action == ActionManager.States.Jump || Action.Action == ActionManager.States.Hovering))
        //{
        //    Debug.Log(MoveInput * AirSkiddingForce * MoveAccell);
        //    setVelocity = HandleGroundControl(1, (MoveInput * AirSkiddingForce) * MoveAccell);
        //}

        if (_moveInput.sqrMagnitude > 0.1f)
        {
            float airMod = 1;
            float airMoveMod = 1;
            if(HorizontalSpeedMagnitude < 15)
            {
                airMod += 2f;
                airMoveMod += 3f;
            }
            if(Action.whatAction == S_Enums.PlayerStates.Jump)
            {
                //Debug.Log(Action.Action01.timeJumping);
                if (Action.Action01.ControlCounter < 0.5)
                {
                    airMod += 1f;
                    airMoveMod += 2f;
                }
                else if(Action.Action01.ControlCounter > 5)
                {
                    airMod -= 1f;
                    airMoveMod -= 4f;
                }
                    
            }
            else if(Action.whatAction == S_Enums.PlayerStates.Bounce)
            {
                airMod += 1f;
                airMoveMod += 2.5f;
            }
            airMoveMod = Mathf.Clamp(airMoveMod, 0.8f, 10);
            airMod = Mathf.Clamp(airMod, 0.8f, 10);

            setVelocity = HandleGroundControl(_airControlAmmount_ * airMod, _moveInput * MoveAccell * airMoveMod);
        }
        else
        {
            setVelocity = HandleGroundControl(_airControlAmmount_, _moveInput * MoveAccell);

            if (_moveInput == Vector3.zero && _shouldStopAirMovementIfNoInput_)
            {
                Vector3 ReducedSpeed = setVelocity;
                ReducedSpeed.x = ReducedSpeed.x / _airDecell_;
                ReducedSpeed.z = ReducedSpeed.z / _airDecell_;
                //setVelocity = ReducedSpeed;
            }

        }
        //Get out of roll
        isRolling = false;


        if (HorizontalSpeedMagnitude > 14)
        {
            Vector3 ReducedSpeed = setVelocity;
            ReducedSpeed.x = ReducedSpeed.x / _naturalAirDecel_;
            ReducedSpeed.z = ReducedSpeed.z / _naturalAirDecel_;
            //setVelocity = ReducedSpeed;
        }

        //Get set for landing
        WasOnAir = true;



        //Apply Gravity
        if (GravityAffects)
            setVelocity += Gravity((int)setVelocity.y);

        //if(setVelocity.y > rb.velocity.y)
        //    Debug.Log("Gravity is = " +Gravity((int)setVelocity.y).y);

        //Max Falling Speed
        if (rb.velocity.y < MaxFallingSpeed)
        {
            setVelocity = new Vector3(setVelocity.x, MaxFallingSpeed, setVelocity.z);
        }

        rb.velocity = setVelocity;


    }
    Vector3 Gravity(int vertSpeed)
    {

        if (vertSpeed < 0)
        {
            return fallGravity;
        }
        else
        {
            int gravMod;
            if (vertSpeed > 70)
                gravMod = vertSpeed / 12;
            else
                gravMod = vertSpeed / 8;
            float applyMod = 1 + (gravMod * 0.1f);

            Vector3 newGrav = new Vector3(0f, _upGravity_.y * applyMod, 0f);

            return newGrav;
        }

    }

    void CheckForGround()
    {
        RayToGroundDistancecor = RayToGroundDistance;
        RayToGroundRotDistancecor = RayToGroundRotDistance;
        if (Action.whatAction == 0 && Grounded)
        {
            //grounder line
            RayToGroundDistancecor = Mathf.Max(RayToGroundDistance + (SpeedMagnitude * RaytoGroundSpeedRatio), RayToGroundDistance);
            RayToGroundDistancecor = Mathf.Min(RayToGroundDistancecor, RaytoGroundSpeedMax);

            //rotorline
            RayToGroundRotDistancecor = Mathf.Max(RayToGroundRotDistance + (SpeedMagnitude * RaytoGroundSpeedRatio), RayToGroundRotDistance);
            RayToGroundRotDistancecor = Mathf.Min(RayToGroundRotDistancecor, RaytoGroundRotSpeedMax);

        }
        if (EnableDebug)
        {
            Debug.DrawRay(transform.position + (transform.up * 2) + -transform.right, -transform.up * (2f + RayToGroundDistancecor), Color.yellow);
        }
        //Debug.Log(GravityAffects);

        if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out groundHit, 2f + RayToGroundDistancecor, _Groundmask_))
        {
            GroundNormal = groundHit.normal;
            Grounded = true;
            GroundMovement();
        }
        else if (Action.whatAction != S_Enums.PlayerStates.Bounce && Action.whatAction != S_Enums.PlayerStates.WallRunning && Action.whatAction != S_Enums.PlayerStates.Rail)
        {
            Grounded = false;
            GroundNormal = Vector3.zero;
            AirMovement();
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawRay(DownRay);
    }

    public void OnCollisionStay(Collision col)
    {
        Vector3 Prevnormal = GroundNormal;
        foreach (ContactPoint contact in col.contacts)
        {

            //Set Middle Point
            Vector3 pointSum = Vector3.zero;
            Vector3 normalSum = Vector3.zero;
            for (int i = 0; i < col.contacts.Length; i++)
            {
                pointSum = pointSum + col.contacts[i].point;
                normalSum = normalSum + col.contacts[i].normal;
            }

            pointSum = pointSum / col.contacts.Length;
            CollisionPointsNormal = normalSum / col.contacts.Length;

            if (rb.velocity.normalized != Vector3.zero)
            {
                CollisionPoint.position = pointSum;
            }

        }
    }

    //Matches all changeable stats to how they are set in the character stats script.
    private void AssignStats()
    {
        _startAcceleration_ = Tools.Stats.AccelerationStats.acceleration;
        _accelOverSpeed_ = Tools.Stats.AccelerationStats.AccelBySpeed;
        _accelShiftOverSpeed_ = Tools.Stats.AccelerationStats.accelShiftOverSpeed;
        _tangentialDrag_ = Tools.Stats.TurningStats.tangentialDrag;
        _tangentialDragShiftSpeed_ = Tools.Stats.TurningStats.tangentialDragShiftSpeed;
        _turnSpeed_ = Tools.Stats.TurningStats.turnSpeed;
     
        _turnRateOverAngle_ = Tools.Stats.TurningStats.TurnRateByAngle;
        _turnRateOverSpeed_ = Tools.Stats.TurningStats.TurnRateBySpeed;
        _tangDragOverAngle_ = Tools.Stats.TurningStats.TangDragByAngle;
        _tangDragOverSpeed_ = Tools.Stats.TurningStats.TangDragBySpeed;
        _startTopSpeed_ = Tools.Stats.SpeedStats.topSpeed;
        _startMaxSpeed_ = Tools.Stats.SpeedStats.maxSpeed;
        _startMaxFallingSpeed_ = Tools.Stats.WhenInAir.startMaxFallingSpeed;
        _startJumpPower_ = Tools.Stats.JumpStats.startJumpSpeed;
        _moveDeceleration_ = Tools.Stats.DecelerationStats.moveDeceleration;
        _decelBySpeed_ = Tools.Stats.DecelerationStats.DecelBySpeed;
        _decelShiftOverSpeed_ = Tools.Stats.DecelerationStats.decelShiftOverSpeed;
        _naturalAirDecel_ = Tools.Stats.DecelerationStats.naturalAirDecel;
        _airDecell_ = Tools.Stats.DecelerationStats.airDecel;
        _groundStickingDistance_ = Tools.Stats.StickToGround.groundStickingDistance;
        _groundStickingPower_ = Tools.Stats.StickToGround.groundStickingPower;
        _slopeEffectLimit_ = Tools.Stats.SlopeStats.slopeEffectLimit;
        _standOnSlopeLimit_ = Tools.Stats.SlopeStats.standOnSlopeLimit;
        _slopePower_ = Tools.Stats.SlopeStats.slopePower;
        _slopeRunningAngleLimit_ = Tools.Stats.SlopeStats.slopeRunningAngleLimit;
        _slopeSpeedLimit_ = Tools.Stats.SlopeStats.SlopeLimitBySpeed;
        _generalHillMultiplier_ = Tools.Stats.SlopeStats.generalHillMultiplier;
        _uphillMultiplier_ = Tools.Stats.SlopeStats.uphillMultiplier;
        _downhillMultiplier_ = Tools.Stats.SlopeStats.downhillMultiplier;
        _startDownhillMultiplier_ = Tools.Stats.SlopeStats.startDownhillMultiplier;
        _slopePowerOverSpeed_ = Tools.Stats.SlopeStats.SlopePowerByCurrentSpeed;
        _airControlAmmount_ = Tools.Stats.WhenInAir.controlAmmount;
        
        _shouldStopAirMovementIfNoInput_ = Tools.Stats.WhenInAir.shouldStopAirMovementWhenNoInput;
        _rollingLandingBoost_ = Tools.Stats.RollingStats.rollingLandingBoost;
        _rollingDownhillBoost_ = Tools.Stats.RollingStats.rollingDownhillBoost;
        _rollingUphillBoost_ = Tools.Stats.RollingStats.rollingUphillBoost;
        _rollingStartSpeed_ = Tools.Stats.RollingStats.rollingStartSpeed;
        _rollingTurningDecrease_ = Tools.Stats.RollingStats.rollingTurningDecrease;
        _rollingFlatDecell_ = Tools.Stats.RollingStats.rollingFlatDecell;
        _slopeTakeoverAmount_ = Tools.Stats.RollingStats.slopeTakeoverAmount;
        _UpHillByTime_ = Tools.Stats.SlopeStats.UpHillEffectByTime;
        _startFallGravity_ = Tools.Stats.WhenInAir.fallGravity;
        _upGravity_ = Tools.Stats.WhenInAir.upGravity;
        _keepNormalForThis_ = Tools.Stats.WhenInAir.keepNormalForThis;


        StickingLerps = Tools.Stats.GreedysStickToGround.stickingLerps;
        StickingNormalLimit = Tools.Stats.GreedysStickToGround.stickingNormalLimit;
        StickCastAhead = Tools.Stats.GreedysStickToGround.stickCastAhead;
        negativeGHoverHeight = Tools.Stats.GreedysStickToGround.negativeGHoverHeight;
        RayToGroundDistance = Tools.Stats.GreedysStickToGround.rayToGroundDistance;
        RaytoGroundSpeedRatio = Tools.Stats.GreedysStickToGround.raytoGroundSpeedRatio;
        RaytoGroundSpeedMax = Tools.Stats.GreedysStickToGround.raytoGroundSpeedMax;
        RayToGroundRotDistance = Tools.Stats.GreedysStickToGround.rayToGroundRotDistance;
        RaytoGroundRotSpeedMax = Tools.Stats.GreedysStickToGround.raytoGroundRotSpeedMax;
        RotationResetThreshold = Tools.Stats.GreedysStickToGround.rotationResetThreshold;

        _Groundmask_ = Tools.Stats.StickToGround.GroundMask;

        //Sets all changeable core values to how they are set to start in the editor.
        MoveAccell = _startAcceleration_;
        TopSpeed = _startTopSpeed_;
        MaxSpeed = _startMaxSpeed_;
        MaxFallingSpeed = _startMaxFallingSpeed_;
        m_JumpPower = _startJumpPower_;
        fallGravity = _startFallGravity_;

        KeepNormal = Vector3.up;


    }

    private void AssignTools()
    {
        MasterPlayer = this;
        rb = GetComponent<Rigidbody>();
        PreviousInput = transform.forward;
        Action = GetComponent<S_ActionManager>();

        CollisionPoint = Tools.CollisionPoint;
        sounds = Tools.SoundControl;
    }
}
