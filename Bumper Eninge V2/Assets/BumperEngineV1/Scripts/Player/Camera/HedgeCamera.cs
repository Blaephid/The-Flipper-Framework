using UnityEngine;
using System.Collections;

public class HedgeCamera : MonoBehaviour
{

    private bool GoingTowardsCamera;

    public CharacterTools Tools;
    public Transform Target;
    public PlayerBhysics Player;
    public Transform Skin;
    public ActionManager Actions;

    bool UseAutoRotation;
    bool UseCurve;
    float AutoXRotationSpeed;
    AnimationCurve AutoXRotationCurve;
    [HideInInspector] public bool LockHeight;
    float LockHeightSpeed;
    bool MoveHeightBasedOnSpeed;
    float HeightToLock;
    float HeightFollowSpeed;
    float FallSpeedThreshold;
    bool facingDown = false;

    [HideInInspector] public float CameraMaxDistance = -11;
    float AngleThreshold;
    bool camOnAngle;

    float CameraRotationSpeed = 100;
    float CameraVerticalRotationSpeed = 10;
    AnimationCurve vertFollowSpeedByAngle;
    float CameraMoveSpeed = 100;

    float InputXSpeed = 1;
    float InputYSpeed = 0.5f;

    float stationaryCamIncrease = 1.2f;

    float yMinLimit = -20f;
    float yMaxLimit = 80f;

    [HideInInspector] public float StartLockCam;
    [HideInInspector] public float LockCamAtHighSpeed = 130;

    //The countdown prevents constant jittering when alternating between the lock cam speed
    bool CanGoBehind = false;
    bool checkforBehind = false;
    bool AboveSpeedLock;

    float x = 0.0f;
    [HideInInspector] public float y = 20.0f;

    float CurveX;

    Quaternion LerpedRot;
    Vector3 LerpedPos;

    float MoveLerpingSpeed;
    float RotationLerpingSpeed;

    public Transform PlayerPosLerped;

    float moveSpeed = 0;
    float rotSpeed = 0;

    public bool Locked { get; set; }
    public bool canMove;
    public bool MasterLocked { get; set; }
    LayerMask CollidableLayers;
    float heighttolook;
    [HideInInspector] public float lookTimer;
    float lookspeed;
    float afterMoveDelay;

    public float LockedRotationSpeed;

    //Cached variables
    float InitialLockedRotationSpeed;

    float lockTimer;
    public bool Reversed;

    //Effects
    Vector3 lookAtDir;

    [Header("ShakeEffects")]
    public static float Shakeforce;
    float ShakeDampen;

    public float InvertedX { get; set; }
    public float InvertedY { get; set; }
    public float SensiX { get; set; }
    public float SensiY { get; set; }

    Vector3 HitNormal;

    void Start()
    {
        Locked = false;
        canMove = true;
        InitialLockedRotationSpeed = LockedRotationSpeed;

        StartLockCam = LockCamAtHighSpeed;
        setStats();

        //Deals with cursor 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {

        CameraMovement();
        CameraRotation();
        CameraCollision();
        CamereApplyEffects();
        CameraSet();
        CameraHighSpeedLock();

    }

    void CameraMovement()
    {
        //LookTimer
        if (lookTimer < 0)
        {
            RotateDirection(lookAtDir, LockedRotationSpeed, heighttolook);
            lookTimer += Time.deltaTime;
        }

        //Copy and Lerp the player's Pos
        //if (!Locked)
        if (true)
        {
            
            if (Player.GroundNormal.y < AngleThreshold || camOnAngle)
            {                             
                CanGoBehind = true;
                AboveSpeedLock = true;

                if (Vector3.Dot(PlayerPosLerped.up, Vector3.up) > 0.999)
                    camOnAngle = false;
                else
                    camOnAngle = true;
            }
            //else if (Player.b_normalSpeed > LockCamAtHighSpeed * 0.5f)
            //{
            //    CanGoBehind = true;
            //    AboveSpeedLock = true;
            //}
            else if (!Player.Grounded)
            {
                CanGoBehind = true;
                AboveSpeedLock = true;

                if (Vector3.Dot(PlayerPosLerped.up, Vector3.up) > 0.999)
                    camOnAngle = false;
                else
                    camOnAngle = true;
            }
            else
            {
                AboveSpeedLock = false;
                CanGoBehind = false;

                if (Vector3.Dot(PlayerPosLerped.up, Vector3.up) > 0.999)
                    camOnAngle = false;
                else
                    camOnAngle = true;

                if (!checkforBehind && AboveSpeedLock)
                {
                    StartCoroutine(SetGoBehind());
                }

            }

            if (CanGoBehind)
            {
                FollowPlayerTransform();
                //FollowPlayerTransformVectors();
            }

            //Debug.Log(PlayerPosLerped.localEulerAngles);


            if (canMove)
            {
                if (Player.SpeedMagnitude > 10)
                {
                    x += (Actions.moveCamX * ((InputXSpeed)) * InvertedX) * Time.deltaTime;
                    y -= (Actions.moveCamY * ((InputYSpeed)) * InvertedY) * Time.deltaTime;
                }
                else
                {
                    x += (Actions.moveCamX * ((InputXSpeed)) * InvertedX) * Time.deltaTime * stationaryCamIncrease;
                    y -= (Actions.moveCamY * ((InputYSpeed)) * InvertedY) * Time.deltaTime * stationaryCamIncrease;
                }
            }

        }
        else
        {
            PlayerPosLerped.rotation = Quaternion.Lerp(PlayerPosLerped.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * CameraVerticalRotationSpeed);
            RotateDirection(lookAtDir, Mathf.RoundToInt(LockedRotationSpeed), heighttolook);
        }

    }


    void CameraRotation()
    {
        if (UseAutoRotation && canMove)
        {

            if (!UseCurve)
            {
                //float NormalMod = Mathf.Abs(Player.b_normalSpeed - Player.MaxSpeed);
                float NormalMod = Mathf.Abs(Player.HorizontalSpeedMagnitude - Player.MaxSpeed);
                //x += (((Input.GetAxis("Horizontal")) * NormalMod) * AutoXRotationSpeed) * Time.deltaTime;
                //;
                //y -= 0;

                x += ((Actions.moveCamX * NormalMod) * AutoXRotationSpeed) * Time.deltaTime;

                y -= 0;

            }
            else
            {

                CurveX = AutoXRotationCurve.Evaluate((Player.rb.velocity.sqrMagnitude / Player.MaxSpeed) / Player.MaxSpeed);
                CurveX = CurveX * 100;
                x += ((Actions.moveCamX * CurveX) * AutoXRotationSpeed) * Time.deltaTime;

                y -= 0;

            }

        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        LerpedRot = Quaternion.Euler(y, x, 0);
        LerpedRot = PlayerPosLerped.rotation * LerpedRot;
    }

    void CameraCollision()
    {
        //Collision

        float dist;
        RaycastHit hit;

        Debug.DrawRay(Target.position, -transform.forward, Color.blue);
        if (Physics.Raycast(Target.position, -transform.forward, out hit, -CameraMaxDistance, CollidableLayers))
        {
            dist = (-hit.distance);
            HitNormal = hit.normal;
        }
        else
        {
            HitNormal = Vector3.zero;
            dist = CameraMaxDistance;
        }


        var position = LerpedRot * new Vector3(0, 0, dist + 0.3f) + Target.position;

        LerpedPos = position;
    }

    void CamereApplyEffects()
    {

        lookTimer += Time.deltaTime;
        if (lookTimer < 0)
        {
            LookAt(lookAtDir);
        }

        if (Locked && lockTimer > 0)
        {
            lockTimer -= Time.deltaTime;
            if (lockTimer < 0)
            {
                Locked = false;
            }
        }

        else
            lockTimer = 0;

        if (MasterLocked)
        {
            Locked = true;
        }

    }

    void LookAt(Vector3 dir)
    {
        RotateDirection(dir, Mathf.RoundToInt(LockedRotationSpeed), y);
    }

    void CameraSet()
    {
        float y = Player.rb.velocity.y;

        if (LockHeight && Player.Grounded && Player.SpeedMagnitude >= 10)
        {
            ////Debug.Log ("Lock");
            FollowHeightDirection(HeightToLock, LockHeightSpeed);
        }

        //Face down
        if (MoveHeightBasedOnSpeed && !Player.Grounded && y < FallSpeedThreshold && (Actions.Action == ActionManager.States.Jump
            || Actions.Action == ActionManager.States.Regular || Actions.Action == ActionManager.States.DropCharge))
        {
            if (!facingDown)
            {
                if (!Physics.Raycast(transform.position, Vector3.down, 20f, 1))
                {
                    //Debug.Log("No hit ground");
                    facingDown = true;
                }
            }

            if (facingDown)
                FollowHeightDirection(-y, HeightFollowSpeed * 0.3f);

        }
        else if (y > FallSpeedThreshold && !Player.Grounded && LockHeight && Actions.moveCamY <= 0.5)
        {
            if (facingDown)
            {
                facingDown = false;
                StartCoroutine(fromDowntoForward());
            }

            ////Sets the camera height to move towards set height at varying speeds
            if (Player.HorizontalSpeedMagnitude > 140)
                FollowHeightDirection(HeightToLock, HeightFollowSpeed * 3f);
            else if (Player.HorizontalSpeedMagnitude > 100)
                FollowHeightDirection(HeightToLock, HeightFollowSpeed * 2.5f);
            else if (Player.HorizontalSpeedMagnitude > 60)
                FollowHeightDirection(HeightToLock, HeightFollowSpeed * 2f);
            else if (Player.HorizontalSpeedMagnitude > 30)
                FollowHeightDirection(HeightToLock, HeightFollowSpeed * 1.6f);
            else if (Player.HorizontalSpeedMagnitude > 10)
                FollowHeightDirection(HeightToLock, HeightFollowSpeed);
        }
        else if (facingDown)
        {
            StartCoroutine(fromDowntoForward());
            facingDown = false;
        }

        moveSpeed = Mathf.Lerp(moveSpeed, CameraMoveSpeed, Time.deltaTime * MoveLerpingSpeed);
        rotSpeed = Mathf.Lerp(rotSpeed, CameraRotationSpeed, Time.deltaTime * RotationLerpingSpeed);

        //transform.position = Vector3.Lerp(transform.position, LerpedPos + HitNormal, Time.deltaTime * moveSpeed);
        //transform.rotation = Quaternion.Lerp(transform.rotation, LerpedRot, Time.deltaTime * rotSpeed);
        transform.position = LerpedPos + HitNormal;
        transform.rotation = LerpedRot;


        //Shakedown!
        float noiseX = (Random.Range(-Shakeforce, Shakeforce));
        float noiseY = (Random.Range(-Shakeforce, Shakeforce));
        float shakeX = (transform.position.x + noiseX);
        float shakeY = (transform.position.y + noiseY);

        transform.position = new Vector3(shakeX, shakeY, transform.position.z);
        Shakeforce = Mathf.Lerp(Shakeforce, 0, Time.deltaTime * ShakeDampen);
    }

    void CameraHighSpeedLock()
    {
        if (Actions.moveCamX >= 0.1 || Actions.moveCamX <= -0.1)
        {
            afterMoveDelay = 0.3f;
        }
        else if (afterMoveDelay > 0)
            afterMoveDelay -= Time.deltaTime;

        if (Player.HorizontalSpeedMagnitude > LockCamAtHighSpeed && (lookTimer > 0) && !GoingTowardsCamera && afterMoveDelay <= 0)
        {
            //Debug.Log("Move camera behind");
            FollowDirection(3, 14, -10, 0);
        }

        if (Player.HorizontalSpeedMagnitude > 30f && Actions.Action == ActionManager.States.WallRunning)
        {
            //Debug.Log("Move camera behind");
            FollowDirection(2, 14, -10, 0.5f);
        }
    }

    public void RotateDirection(Vector3 dir, float speed, float height)
    {
        float dot = Vector3.Dot(dir, transform.right);
        //Debug.Log("RotateDirection");
        x += (dot * speed) * (Time.deltaTime * 100);
        y = Mathf.Lerp(y, height, Time.deltaTime * 5);

    }

    //Constnaly moves camera to behind player
    public void FollowDirection(float speed, float height, float distance, float Yspeed, bool Skip = false)
    {
        if (Reversed)
        {
            FollowDirectionBehind(speed, height, distance, Yspeed, Skip);
        }

        else if (!Locked || Skip)
        {
            float dot = Vector3.Dot(Skin.forward, transform.right);
            x += (dot * speed) * (Time.deltaTime * 100);
            // x = Mathf.MoveTowards(x, Skin.eulerAngles.y, (dot * speed) * (Time.deltaTime * 60));

            y = Mathf.Lerp(y, height, Time.deltaTime * Yspeed);
        }
    }

    public void FollowDirectionBehind(float speed, float height, float distance, float Yspeed, bool Skip = false)
    {
        if (true)
        {
            float dot = Vector3.Dot(-Skin.forward, transform.right);
            x += (dot * speed) * (Time.deltaTime * 100);

            //Quaternion inverse = Quaternion.LookRotation(-Skin.forward, Skin.up);
            //x = Mathf.MoveTowards(x, inverse.eulerAngles.y, (dot * speed) * (Time.deltaTime * 60));

            y = Mathf.Lerp(y, height, Time.deltaTime * Yspeed);
        }
    }

    public void setBehind()
    {
        x = Skin.eulerAngles.y;

    }

    public void setBehindWithHeight()
    {
        x = Skin.eulerAngles.y;
        y = 14;
    }

    public void FollowHeightDirection(float height, float speed)
    {
        if (!Locked)
        {
            if (Actions.Action != ActionManager.States.Rail)
            {
                if (Player.Grounded)
                {
                    y = Mathf.Lerp(y, height, Time.deltaTime * speed);
                }
                else
                {
                    y = Mathf.Lerp(y, height, Time.deltaTime * (speed / 6));
                }
            }
        }
    }

    public float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;

        return Mathf.Clamp(angle, min, max);
    }

    public void lockCamFor(float time)
    {
        Locked = true;
        lockTimer = time;
    }

    //Set camera and overloads (function with the same name are different options)

    public void SetCamera(Vector3 dir, float duration, float heightSet)
    {

        lookAtDir = dir;
        lookTimer = -duration;
        heighttolook = heightSet;
        LockedRotationSpeed = InitialLockedRotationSpeed * 0.01f;

    }
    public void SetCamera(Vector3 dir, float duration, float heightSet, float speed)
    {

        lookAtDir = dir;
        lookTimer = -duration;
        heighttolook = heightSet;
        LockedRotationSpeed = speed;

    }
    public void SetCameraNoHeight(Vector3 dir, float duration, float speed)
    {
        lookAtDir = dir;
        lookTimer = -duration;
        heighttolook = y;
        LockedRotationSpeed = speed;
    }

    public void SetCamera(Vector3 dir, float duration, float heightSet, float speed, float lagSet)
    {

        lookAtDir = dir;
        lookTimer = -duration;
        heighttolook = heightSet;
        LockedRotationSpeed = speed;
        moveSpeed = lagSet;
        rotSpeed = lagSet * 0.1f;

    }

    public void SetCameraNoLook(float heightSet)
    {
        heighttolook = heightSet;
    }

    public void SetCamera(Vector3 dir, bool instant)
    {
        float dot = Vector3.Angle(dir, transform.forward);
        x += dot;

    }
    public void SetCamera(float lagSet)
    {

        moveSpeed = lagSet;
        rotSpeed = lagSet * 0.1f;

    }

    public void CamLagSet(float moveLagSet, float rotationLagSet = 1)
    {
        //if (Timer < GiveControlTime) { return; } // deny Function if player is moving camera
        //CameraMoveTime = CameraMoveTime * moveLagSet;
        //rotSpeed = RotationLerpingSpeed * rotationLagSet;

    }

    IEnumerator SetGoBehind()
    {
        checkforBehind = true;
        yield return new WaitForSeconds(.5f);
        if (!AboveSpeedLock)
            CanGoBehind = false;
        checkforBehind = false;

    }

    public void FollowPlayerTransform()
    {

        PlayerPosLerped.position = Target.position;
        Quaternion targetRot = Quaternion.LookRotation(transform.forward, Player.transform.up);
        targetRot = Player.transform.rotation;
        Quaternion newRot = PlayerPosLerped.rotation;
        //newRot = PlayerPosLerped.rotation * Player.transform.rotation;
        //newRot = Quaternion.LookRotation(transform.forward, PlayerPosLerped.up);

        //Debug.Log(PlayerPosLerped.localEulerAngles);

        float heightMod = vertFollowSpeedByAngle.Evaluate(Quaternion.Angle(newRot, targetRot) / 18);

        if (!Player.Grounded)
        {

            newRot = Quaternion.Slerp(newRot, targetRot, Time.deltaTime * CameraVerticalRotationSpeed * heightMod);

        }
        else
        {
            newRot = Quaternion.Slerp(newRot, targetRot, Time.deltaTime * CameraVerticalRotationSpeed * heightMod);
        }


        PlayerPosLerped.rotation = newRot;

        //if (PlayerPosLerped.rotation != new Quaternion(0, 0, 0, -1))
        //{
        //    if (Player.Grounded)
        //        Debug.Log("Grounded with+ " + PlayerPosLerped.rotation);
        //    else
        //        Debug.Log("Air with- " + PlayerPosLerped.rotation);

        //    Debug.Log("Target is= " + targetRot);

        //}


    }

    public void FollowPlayerTransformVectors()
    {

        PlayerPosLerped.position = Target.position;
        Vector3 targetRot = Player.transform.rotation.eulerAngles;
        Vector3 newRot = PlayerPosLerped.rotation.eulerAngles;

        float heightMod = vertFollowSpeedByAngle.Evaluate(Vector3.Angle(newRot, targetRot) / 18);

        if (!Player.Grounded)
        {

            newRot = Vector3.Slerp(newRot, targetRot, Time.deltaTime * CameraVerticalRotationSpeed * heightMod);

        }
        else
        {
            newRot = Vector3.Slerp(newRot, targetRot, Time.deltaTime * CameraVerticalRotationSpeed * heightMod);
        }


        PlayerPosLerped.rotation = Quaternion.Euler(newRot);

        if (PlayerPosLerped.rotation != new Quaternion(0, 0, 0, -1))
        {
            if (Player.Grounded)
                Debug.Log("Grounded with+ " + PlayerPosLerped.rotation.eulerAngles);
            else
                Debug.Log("Air with- " + PlayerPosLerped.rotation.eulerAngles);

            Debug.Log("Target is= " + targetRot);

        }


    }

    IEnumerator fromDowntoForward()
    {
        //Debug.Log("fromDowntoForward");
        float initialFollow = HeightFollowSpeed;
        HeightFollowSpeed *= 4f;
        yield return new WaitForSeconds(.8f);
        HeightFollowSpeed = initialFollow;
    }

    void setStats()
    {
        UseAutoRotation = Tools.camStats.UseAutoRotation;
        UseCurve = Tools.camStats.UseCurve;
        AutoXRotationSpeed = Tools.camStats.AutoXRotationSpeed;
        AutoXRotationCurve = Tools.camStats.AutoXRotationCurve;
        LockHeight = Tools.camStats.LockHeight;
        LockHeightSpeed = Tools.camStats.LockHeightSpeed;
        MoveHeightBasedOnSpeed = Tools.camStats.MoveHeightBasedOnSpeed;
        HeightToLock = Tools.camStats.HeightToLock;
        HeightFollowSpeed = Tools.camStats.HeightFollowSpeed;
        FallSpeedThreshold = Tools.camStats.FallSpeedThreshold;

        CameraMaxDistance = Tools.camStats.CameraMaxDistance;
        AngleThreshold = Tools.camStats.AngleThreshold;

        CameraRotationSpeed = Tools.camStats.CameraRotationSpeed;
        CameraVerticalRotationSpeed = Tools.camStats.CameraVerticalRotationSpeed;
        vertFollowSpeedByAngle = Tools.camStats.vertFollowSpeedByAngle;
        CameraMoveSpeed = Tools.camStats.CameraMoveSpeed;

        InputXSpeed = Tools.camStats.InputXSpeed;
        InputYSpeed = Tools.camStats.InputYSpeed;
        stationaryCamIncrease = Tools.camStats.stationaryCamIncrease;

        yMinLimit = Tools.camStats.yMinLimit;
        yMaxLimit = Tools.camStats.yMaxLimit;

        LockCamAtHighSpeed = Tools.camStats.LockCamAtHighSpeed;

        MoveLerpingSpeed = Tools.camStats.MoveLerpingSpeed;
        RotationLerpingSpeed = Tools.camStats.RotationLerpingSpeed;

        LockedRotationSpeed = Tools.camStats.LockedRotationSpeed;
        ShakeDampen = Tools.camStats.ShakeDampen;

        CollidableLayers = Tools.camStats.CollidableLayers;
    }

}




