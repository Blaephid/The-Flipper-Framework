using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerBhysics : MonoBehaviour
{
    [HideInInspector] public ActionManager Action;
    CharacterTools Tools;

    static public PlayerBhysics MasterPlayer;

    [Header("Movement Values")]

    [HideInInspector] public float StartAccell = 0.5f;
    [HideInInspector] public float MoveAccell;

    [HideInInspector] public AnimationCurve AccellOverSpeed;
    [HideInInspector] public float AccellShiftOverSpeed;
    float DecellShiftOverSpeed;

    [HideInInspector] public float MoveDecell = 1.3f;
    AnimationCurve DecellBySpeed;
    [HideInInspector] public float AirDecell = 1.05f;
    [HideInInspector] public float naturalAirDecell = 1.01f;

    [HideInInspector] public float TangentialDrag;
    [HideInInspector] public float TangentialDragShiftSpeed;

    [HideInInspector] public float TurnSpeed = 16f;
    [HideInInspector] public float SlowedTurnSpeed = 500f;

    [HideInInspector] public AnimationCurve TurnRateOverAngle;
    [HideInInspector] public AnimationCurve TurnRateOverSpeed;
    [HideInInspector] public AnimationCurve TangDragOverAngle;
    [HideInInspector] public AnimationCurve TangDragOverSpeed;

    [HideInInspector] public float StartTopSpeed = 65f;
    [HideInInspector] public float TopSpeed;
    [HideInInspector] public float StartMaxSpeed = 230f;
    [HideInInspector] public float MaxSpeed;
    [HideInInspector] public float StartMaxFallingSpeed = -500f;
    [HideInInspector] public float MaxFallingSpeed;
    [HideInInspector] public float StartJumpPower = 2;
    [HideInInspector] public float m_JumpPower;

    [HideInInspector] public float GroundStickingDistance = 1;
    [HideInInspector] public float GroundStickingPower = -1;

    [HideInInspector] public float SlopeEffectLimit = 0.9f;
    [HideInInspector] public float StandOnSlopeLimit = 0.8f;
    [HideInInspector] public float SlopePower = 0.5f;
    [HideInInspector] public float SlopeRunningAngleLimit = 0.5f;
    [HideInInspector] public float SlopeSpeedLimit = 10;

    float generalHillMultiplier = 1;
    [HideInInspector] public float UphillMultiplier = 0.5f;
    [HideInInspector] public float DownhillMultiplier = 2;
    [HideInInspector] public float StartDownhillMultiplier = -7;

    [HideInInspector] public AnimationCurve SlopePowerOverSpeed;
    [HideInInspector] public AnimationCurve UpHillOverTime;
    [HideInInspector] public float SlopePowerShiftSpeed;
    [HideInInspector] public float LandingConversionFactor = 2;

    [Header("AirMovementExtras")]
    [HideInInspector] public float AirControlAmmount = 2;
    [HideInInspector] public float AirSkiddingForce = 10;
    [HideInInspector] public bool StopAirMovementIfNoInput = false;
    float keepNormalForThis = 0.083f;


    public bool Grounded { get; set; }
    public Vector3 GroundNormal { get; set; }
    public Vector3 CollisionPointsNormal { get; set; }

    public Rigidbody rb { get; set; }

    [HideInInspector] public bool GravityAffects = true;
    [HideInInspector] public Vector3 StartFallGravity;
    [HideInInspector] public Vector3 fallGravity;
    Vector3 UpGravity;
    public Vector3 MoveInput { get; set; }


    [Header("Other Values")]

    public float GroundOffset;
    [HideInInspector] public RaycastHit groundHit;
    public Transform CollisionPoint;
    public Collider CollisionSphere;
    public Collider CollisionCapsule;
    public PullItems itemPull;

    public Transform MainCamera;
    public Transform Colliders;
    public SonicSoundsControl sounds;
    [HideInInspector] public float HomingDelay;


    public DebugUI Debui;


    [Header("Rolling Values")]

    [HideInInspector] public float RollingLandingBoost;
    [HideInInspector] public float RollingDownhillBoost;
    [HideInInspector] public float RollingUphillBoost;
    [HideInInspector] public float RollingStartSpeed;
    [HideInInspector] public float RollingTurningDecreace;
    [HideInInspector] public float RollingFlatDecell;
    [HideInInspector] public float SlopeTakeoverAmount; // This is the normalized slope angle that the player has to be in order to register the land as "flat"
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
    [Header("Etc Values")]

    public bool UseSphereToGetNormal;

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

    [HideInInspector] public LayerMask Playermask;



    private void Start()
    {
        MasterPlayer = this;
        rb = GetComponent<Rigidbody>();
        PreviousInput = transform.forward;
        Action = GetComponent<ActionManager>();
        Tools = GetComponent<CharacterTools>();
        AssignStats();      

    }

    void FixedUpdate()
    {
        //Debug.Log(GroundNormal);
        //Debug.Log(Action.Action);
        //Debug.Log(isRolling);
        //Debug.Log(HorizontalSpeedMagnitude);


        TimeOnGround += Time.deltaTime;
        if (!Grounded) TimeOnGround = 0;
        GeneralPhysics();

        if (HomingDelay > 0)
        {
            HomingDelay -= Time.deltaTime;
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

        if (MoveInput.sqrMagnitude >= 0.9f)
        {
            PreviousInput = MoveInput;
        }
        if (RawInput.sqrMagnitude >= 0.9f)
        {
            PreviousRawInput = RawInput;
        }

        //Set Curve thingies
        curvePosAcell = Mathf.Lerp(curvePosAcell, AccellOverSpeed.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * AccellShiftOverSpeed);
        curvePosDecell = Mathf.Lerp(curvePosDecell, DecellBySpeed.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * DecellShiftOverSpeed);
        curvePosTang = Mathf.Lerp(curvePosTang, TangDragOverSpeed.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * TangentialDragShiftSpeed);
        curvePosSlope = Mathf.Lerp(curvePosSlope, SlopePowerOverSpeed.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed), Time.fixedDeltaTime * SlopePowerShiftSpeed);

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

        //if ((Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out hitRot, 2f + RayToGroundRotDistancecor, Playermask)))
        if(Grounded)
        {
            //GroundNormal = hit.normal;
            GroundNormal = groundHit.normal;
            //transform.rotation = Quaternion.FromToRotation(transform.up, GroundNormal) * transform.rotation;
            transform.up = GroundNormal;
            //transform.rotation = Quaternion.FromToRotation(transform.up, GroundNormal);


            KeepNormal = GroundNormal;
            KeepNormalCounter = 0;
        }
        else
        {
            //Keep the rotation after exiting the ground for a while, to avoid collision issues.

            KeepNormalCounter += Time.deltaTime;
            if (KeepNormalCounter < keepNormalForThis)
            {
                //transform.rotation = Quaternion.FromToRotation(transform.up, KeepNormal) * transform.rotation;
                transform.up = KeepNormal;
                //transform.rotation = Quaternion.FromToRotation(transform.up, KeepNormal);
            }
            else
            {
                //transform.up = Vector3.RotateTowards(transform.up, Vector3.up, 0.25f, 0);
                if (transform.up.y < RotationResetThreshold)
                {
                    //transform.rotation = Quaternion.identity;
                    transform.up = Vector3.RotateTowards(transform.up, Vector3.up, 0.2f, 0);
                    if (EnableDebug)
                    {
                        Debug.Log("reset");
                    }
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
                }
            }
        }
        CheckForGround();
    }

    Vector3 HandleGroundControl(float deltaTime, Vector3 input)
    {
        if(Action.Action != ActionManager.States.JumpDash && Action.Action != ActionManager.States.WallRunning)
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
              
                float turnRate = TurnRateOverAngle.Evaluate(deviationFromInput);
                turnRate *= TurnRateOverSpeed.Evaluate((rb.velocity.sqrMagnitude / MaxSpeed) / MaxSpeed);
                //lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, Mathf.Deg2Rad * TurnSpeed * turnRate * Time.deltaTime, 0.0f);
                lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, TurnSpeed * turnRate * Time.deltaTime, 0.0f);
                


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

                float dragRate = TangDragOverAngle.Evaluate(deviationFromInput)
                                * TangDragOverSpeed.Evaluate((tangentSpeed * tangentSpeed) / (MaxSpeed * MaxSpeed));

                tangentVelocity = Vector3.MoveTowards(tangentVelocity, Vector3.zero, TangentialDrag * dragRate * deltaTime);

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
                if (isRolling && GroundNormal.y > SlopeTakeoverAmount && HorizontalSpeedMagnitude > 10)
                {
                    DecellAmount = RollingFlatDecell * curvePosDecell;
                    if (input.sqrMagnitude == 0)
                        DecellAmount *= MoveDecell;
                }

                else if (input.sqrMagnitude == 0)
                {
                    DecellAmount = MoveDecell * curvePosDecell;
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

        Vector3 setVelocity = HandleGroundControl(1, MoveInput * curvePosAcell);
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
                Addsped = (GroundNormal * LandingConversionFactor) * RollingLandingBoost;
                //StickToGround(GroundStickingPower * RollingLandingBoost);
                sounds.SpinningSound();
            }

            Addsped.y = 0;
            AddVelocity(Addsped);
            WasOnAir = false;
        }

        //Get out of slope if speed is too low
        if (rb.velocity.sqrMagnitude < SlopeSpeedLimit && SlopeRunningAngleLimit > GroundNormal.y)
        {
            transform.rotation = Quaternion.identity;
            AddVelocity(GroundNormal * 3);
        }
    


        //Apply slope power
        if (GroundNormal.y < SlopeEffectLimit)
        {

            if (timeUpHill < 0)
                timeUpHill = 0;

            if (rb.velocity.y > StartDownhillMultiplier)
            {
                timeUpHill += Time.deltaTime;
                //Debug.Log(p_rigidbody.velocity.y);
                if (!isRolling)
                {
                    Vector3 force = new Vector3(0, (SlopePower * curvePosSlope) * UphillMultiplier * generalHillMultiplier, 0);
                    force *= UpHillOverTime.Evaluate(timeUpHill);
                    AddVelocity(force);
                }
                else
                {
                    Vector3 force = new Vector3(0, (SlopePower * curvePosSlope) * UphillMultiplier * generalHillMultiplier, 0) * RollingUphillBoost;
                    force *= UpHillOverTime.Evaluate(timeUpHill);
                    AddVelocity(force);
                }
            }

            else
            {
                timeUpHill -= Time.deltaTime * 0.8f;
                if (MoveInput != Vector3.zero && b_normalSpeed > 0)
                {
                    if (!isRolling)
                    {
                        Vector3 force = new Vector3(0, (SlopePower * curvePosSlope) * DownhillMultiplier * generalHillMultiplier, 0);
                        AddVelocity(force);
                    }
                    else
                    {
                        Vector3 force = new Vector3(0, (SlopePower * curvePosSlope) * DownhillMultiplier * generalHillMultiplier, 0) * RollingDownhillBoost;
                        AddVelocity(force);
                    }

                }
                else if (GroundNormal.y < StandOnSlopeLimit)
                {
                    Vector3 force = new Vector3(0, SlopePower * curvePosSlope, 0);
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
            Debug.DrawRay(Raycasterpos, rb.velocity * StickCastAhead * Time.deltaTime, Color.black, 1);
            if (Physics.Raycast(Raycasterpos, rb.velocity.normalized, out hitSticking, SpeedMagnitude * StickCastAhead * Time.deltaTime, Playermask))
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
                    if (Physics.Raycast(Raycasterpos + (rb.velocity * StickCastAhead * Time.deltaTime), -groundHit.normal, out hitSticking, 2.5f, Playermask))
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

        if (MoveInput.sqrMagnitude > 0.1f)
        {
            float airMod = 1;
            float airMoveMod = 1;
            if(HorizontalSpeedMagnitude < 15)
            {
                airMod += 2f;
                airMoveMod += 3f;
            }
            if(Action.Action == ActionManager.States.Jump)
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
            else if(Action.Action == ActionManager.States.Bounce)
            {
                airMod += 1f;
                airMoveMod += 2.5f;
            }
            airMoveMod = Mathf.Clamp(airMoveMod, 0.8f, 10);
            airMod = Mathf.Clamp(airMod, 0.8f, 10);

            setVelocity = HandleGroundControl(AirControlAmmount * airMod, MoveInput * MoveAccell * airMoveMod);
        }
        else
        {
            setVelocity = HandleGroundControl(AirControlAmmount, MoveInput * MoveAccell);

            if (MoveInput == Vector3.zero && StopAirMovementIfNoInput)
            {
                Vector3 ReducedSpeed = setVelocity;
                ReducedSpeed.x = ReducedSpeed.x / AirDecell;
                ReducedSpeed.z = ReducedSpeed.z / AirDecell;
                //setVelocity = ReducedSpeed;
            }

        }
        //Get out of roll
        isRolling = false;


        if (HorizontalSpeedMagnitude > 14)
        {
            Vector3 ReducedSpeed = setVelocity;
            ReducedSpeed.x = ReducedSpeed.x / naturalAirDecell;
            ReducedSpeed.z = ReducedSpeed.z / naturalAirDecell;
            //setVelocity = ReducedSpeed;
        }

        //Get set for landing
        WasOnAir = true;



        //Apply Gravity
        if (GravityAffects)
            setVelocity += Gravity((int)setVelocity.y);

        if(setVelocity.y > rb.velocity.y)
            Debug.Log("Gravity is = " +Gravity((int)setVelocity.y).y);

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

            Vector3 newGrav = new Vector3(0f, UpGravity.y * applyMod, 0f);

            return newGrav;
        }

    }

    void CheckForGround()
    {
        RayToGroundDistancecor = RayToGroundDistance;
        RayToGroundRotDistancecor = RayToGroundRotDistance;
        if (Action.Action == 0 && Grounded)
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

        if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out groundHit, 2f + RayToGroundDistancecor, Playermask) && Action.Action != ActionManager.States.Bounce)
        {
            GroundNormal = groundHit.normal;
            Grounded = true;
            GroundMovement();
        }
        else if (Action.Action != ActionManager.States.Bounce && Action.Action != ActionManager.States.WallRunning && Action.Action != ActionManager.States.Rail)
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
            Debug.DrawRay(contact.point, contact.normal, Color.white);

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
        StartAccell = Tools.stats.StartAccell;
        AccellOverSpeed = Tools.coreStats.AccellOverSpeed;
        AccellShiftOverSpeed = Tools.coreStats.AccellShiftOverSpeed;
        TangentialDrag = Tools.stats.TangentialDrag;
        TangentialDragShiftSpeed = Tools.coreStats.TangentialDragShiftSpeed;
        TurnSpeed = Tools.stats.TurnSpeed;
        SlowedTurnSpeed = Tools.stats.SlowedTurnSpeed;
        TurnRateOverAngle = Tools.coreStats.TurnRateOverAngle;
        TurnRateOverSpeed = Tools.coreStats.TurnRateOverSpeed;
        TangDragOverAngle = Tools.coreStats.TangDragOverAngle;
        TangDragOverSpeed = Tools.coreStats.TangDragOverSpeed;
        StartTopSpeed = Tools.stats.StartTopSpeed;
        StartMaxSpeed = Tools.stats.StartMaxSpeed;
        StartMaxFallingSpeed = Tools.stats.StartMaxFallingSpeed;
        StartJumpPower = Tools.stats.StartJumpPower;
        MoveDecell = Tools.stats.MoveDecell;
        DecellBySpeed = Tools.coreStats.DecellBySpeed;
        DecellShiftOverSpeed = Tools.coreStats.DecellShiftOverSpeed;
        naturalAirDecell = Tools.coreStats.naturalAirDecell;
        AirDecell = Tools.stats.AirDecell;
        GroundStickingDistance = Tools.coreStats.GroundStickingDistance;
        GroundStickingPower = Tools.coreStats.GroundStickingPower;
        SlopeEffectLimit = Tools.coreStats.SlopeEffectLimit;
        StandOnSlopeLimit = Tools.coreStats.StandOnSlopeLimit;
        SlopePower = Tools.coreStats.SlopePower;
        SlopeRunningAngleLimit = Tools.coreStats.SlopeRunningAngleLimit;
        SlopeSpeedLimit = Tools.coreStats.SlopeSpeedLimit;
        generalHillMultiplier = Tools.stats.generalHillMultiplier;
        UphillMultiplier = Tools.coreStats.UphillMultiplier;
        DownhillMultiplier = Tools.coreStats.DownhillMultiplier;
        StartDownhillMultiplier = Tools.coreStats.StartDownhillMultiplier;
        SlopePowerOverSpeed = Tools.coreStats.SlopePowerOverSpeed;
        AirControlAmmount = Tools.stats.AirControlAmmount;
        AirSkiddingForce = Tools.stats.AirSkiddingForce;
        StopAirMovementIfNoInput = Tools.coreStats.StopAirMovementIfNoInput;
        RollingLandingBoost = Tools.coreStats.RollingLandingBoost;
        RollingDownhillBoost = Tools.coreStats.RollingDownhillBoost;
        RollingUphillBoost = Tools.coreStats.RollingUphillBoost;
        RollingStartSpeed = Tools.coreStats.RollingStartSpeed;
        RollingTurningDecreace = Tools.coreStats.RollingTurningDecreace;
        RollingFlatDecell = Tools.coreStats.RollingFlatDecell;
        SlopeTakeoverAmount = Tools.coreStats.SlopeTakeoverAmount;
        UpHillOverTime = Tools.coreStats.UpHillOverTime;
        StartFallGravity = Tools.stats.fallGravity;
        UpGravity = Tools.coreStats.UpGravity;
        keepNormalForThis = Tools.coreStats.keepNormalForThis;


        StickingLerps = Tools.coreStats.StickingLerps;
        StickingNormalLimit = Tools.coreStats.StickingNormalLimit;
        StickCastAhead = Tools.coreStats.StickCastAhead;
        negativeGHoverHeight = Tools.coreStats.negativeGHoverHeight;
        RayToGroundDistance = Tools.coreStats.RayToGroundDistance;
        RaytoGroundSpeedRatio = Tools.coreStats.RaytoGroundSpeedRatio;
        RaytoGroundSpeedMax = Tools.coreStats.RaytoGroundSpeedMax;
        RayToGroundRotDistance = Tools.coreStats.RayToGroundRotDistance;
        RaytoGroundRotSpeedMax = Tools.coreStats.RaytoGroundRotSpeedMax;
        RotationResetThreshold = Tools.coreStats.RotationResetThreshold;

        Playermask = Tools.coreStats.Playermask;

        //Sets all changeable core values to how they are set to start in the editor.
        MoveAccell = StartAccell;
        TopSpeed = StartTopSpeed;
        MaxSpeed = StartMaxSpeed;
        MaxFallingSpeed = StartMaxFallingSpeed;
        m_JumpPower = StartJumpPower;
        fallGravity = StartFallGravity;


    }
}
