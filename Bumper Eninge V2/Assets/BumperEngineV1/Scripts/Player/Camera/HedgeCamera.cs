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

    Transform CameraTrans;
    public bool UseAutoRotation;
    public bool UseCurve;
    public float AutoXRotationSpeed;
    public AnimationCurve AutoXRotationCurve;
    public bool LockHeight;
    public float LockHeightSpeed;
    public bool MoveHeightBasedOnSpeed;
    public float HeightToLock;
    public float HeightFollowSpeed;
    public float FallSpeedThreshold;
    bool facingDown = false;

    public float CameraMaxDistance = -11;
    public float CameraYOffset = 3;
    public float AngleThreshold;

    public float CameraRotationSpeed = 100;
    public float CameraVerticalRotationSpeed = 10;
    public float CameraNormalRotationSpeed = 2;
    public float CameraMoveSpeed = 100;

    public float InputXSpeed = 1;
    public float InputYSpeed = 0.5f;
    public float InputSensi = 1f;
    public float InputMouseSensi = 1.1f;
    public float stationaryCamIncrease = 1.2f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [HideInInspector] public float StartLockCam;
    public float LockCamAtHighSpeed = 130;

    //The countdown prevents constant jittering when alternating between the lock cam speed
    bool CanGoBehind = false;
    bool checkforBehind = false;
    bool AboveSpeedLock;

    float x = 0.0f;
    [HideInInspector] public float y = 20.0f;
    float z = 0.0f;
    Vector3 LerpedNormal;

    float AutoX;
    float CurveX;

    Quaternion LerpedRot;
    Vector3 LerpedPos;

    public float MoveLerpingSpeed;
    public float RotationLerpingSpeed;

    public Transform PlayerPosLerped;

    float moveSpeed = 0;
    float rotSpeed = 0;

    public bool Locked { get; set; }
    public bool MasterLocked { get; set; }
    public LayerMask CollidableLayers;
    float heighttolook;
    float lookTimer;
    float lookspeed;
    float afterMoveDelay;

    public float LockedRotationSpeed;

    //Cached variables
    Quaternion rotation;
    float InitialLockedRotationSpeed;
    float InitialRotationSpeed;
    float InitialMoveSpeed;
    float newX;
    float neoX;
    float lockTimer;

    //Effects
    float lookAtCount = 0;
    Vector3 lookAtDir;

    [Header("ShakeEffects")]
    public static float Shakeforce;
    public float ShakeDampen;

    public float InvertedX { get; set; }
    public float InvertedY { get; set; }
    public float SensiX { get; set; }
    public float SensiY { get; set; }

    Vector3 HitNormal;

    void Start()
    {
        Locked = false;
        InitialLockedRotationSpeed = LockedRotationSpeed;
        InitialMoveSpeed = moveSpeed;
        InitialRotationSpeed = rotSpeed;
        StartLockCam = LockCamAtHighSpeed;

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
           // Debug.Log("Looking towards");
           // Debug.Log(lookAtDir);
           // Debug.Log(LockedRotationSpeed);
           // Debug.Log(heighttolook);
            RotateDirection(lookAtDir, LockedRotationSpeed, heighttolook);
            lookTimer += Time.deltaTime;
        }

        //Copy and Lerp the player's Pos
        if (!Locked)
        {

            if (Player.GroundNormal.y < AngleThreshold && Player.Grounded || Player.b_normalSpeed > LockCamAtHighSpeed)
            {

                AboveSpeedLock = true;
                CanGoBehind = true;
                
                
            }

            else
            {
               
                if (!checkforBehind && AboveSpeedLock)
                {
                    StartCoroutine(SetGoBehind());
                }

                AboveSpeedLock = false;

                PlayerPosLerped.position = Target.position;
                Quaternion newrot = Player.transform.rotation;
                newrot.eulerAngles = new Vector3(0, newrot.eulerAngles.y, 0);
                PlayerPosLerped.rotation = Quaternion.Lerp(PlayerPosLerped.rotation, newrot, Time.deltaTime * CameraVerticalRotationSpeed);
            }

            if (CanGoBehind)
            {
                GoBehind();
            }

            //Debug.Log(Actions.moveCamX);
            
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
        else
        {
            PlayerPosLerped.rotation = Quaternion.Lerp(PlayerPosLerped.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * CameraVerticalRotationSpeed);
            RotateDirection(lookAtDir, Mathf.RoundToInt(LockedRotationSpeed), heighttolook);
        }
        z = 0;
    }

    IEnumerator SetGoBehind()
    {
        checkforBehind = true;
        yield return new WaitForSeconds(.5f);
        if (!AboveSpeedLock)
            CanGoBehind = false;
        checkforBehind = false;

    }


    void CameraRotation()
    {
        if (UseAutoRotation && !Locked)
        {

            if (!UseCurve)
            {
                float NormalMod = Mathf.Abs(Player.b_normalSpeed - Player.MaxSpeed);
                //x += (((Input.GetAxis("Horizontal")) * NormalMod) * AutoXRotationSpeed) * Time.deltaTime;
                //;
                //y -= 0;
                //z = 0;
                x += ((Actions.moveCamX * NormalMod) * AutoXRotationSpeed) * Time.deltaTime;
                
                y -= 0;
                z = 0;
            }
            else
            {

                CurveX = AutoXRotationCurve.Evaluate((Player.rb.velocity.sqrMagnitude / Player.MaxSpeed) / Player.MaxSpeed);
                CurveX = CurveX * 100;
                x += ((Actions.moveCamX * CurveX) * AutoXRotationSpeed) * Time.deltaTime;
                
                y -= 0;
                z = 0;
            }

        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        rotation = Quaternion.Euler(y, x, 0);
        rotation = PlayerPosLerped.rotation * rotation;
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


        var position = rotation * new Vector3(0, 0, dist + 0.3f) + Target.position;

        LerpedRot = rotation;
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
        //bool GoingTowardsCamera = false;
        /*
		if (Vector3.Dot (transform.forward, Player.rigidbody.velocity) < -10) {
			GoingTowardsCamera = true;

			////Debug.Log ("TowardsCamera");

		} else 
		{
			GoingTowardsCamera = false;
			////Debug.Log ("AwayFromCamera");
		}

		if (Player.Grounded && GoingTowardsCamera && Player.SpeedMagnitude <= -10)
		{
			FollowDirection(yMaxLimit,LockHeightSpeed/10);
		}*/
        if (LockHeight && Player.Grounded && Player.SpeedMagnitude >= 10)
        {
            ////Debug.Log ("Lock");
            FollowDirection(HeightToLock, LockHeightSpeed);
        }

        //Face down
        if (MoveHeightBasedOnSpeed && !Player.Grounded && y < FallSpeedThreshold && (Actions.Action == 1 || Actions.Action == 0 || Actions.Action == 8))
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
                FollowDirection(-y, HeightFollowSpeed * 0.3f);

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
                FollowDirection(HeightToLock, HeightFollowSpeed * 3f);
            else if (Player.HorizontalSpeedMagnitude > 100)
                FollowDirection(HeightToLock, HeightFollowSpeed * 2.5f);
            else if (Player.HorizontalSpeedMagnitude > 60)
                FollowDirection(HeightToLock, HeightFollowSpeed * 2f);
            else if (Player.HorizontalSpeedMagnitude > 30)
                FollowDirection(HeightToLock, HeightFollowSpeed * 1.6f);
            else if (Player.HorizontalSpeedMagnitude > 10)
                FollowDirection(HeightToLock, HeightFollowSpeed);
        }
        else if (facingDown)
        {
            StartCoroutine(fromDowntoForward());
            facingDown = false;
        }

        moveSpeed = Mathf.Lerp(moveSpeed, CameraMoveSpeed, Time.deltaTime * MoveLerpingSpeed);
        rotSpeed = Mathf.Lerp(rotSpeed, CameraRotationSpeed, Time.deltaTime * RotationLerpingSpeed);

        transform.position = Vector3.Lerp(transform.position, LerpedPos + HitNormal, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, LerpedRot, Time.deltaTime * rotSpeed);

        //transform.position = LerpedPos + HitNormal;
        //transform.rotation = LerpedRot;


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
            afterMoveDelay = 0.4f;
        }
        else if (afterMoveDelay > 0)
            afterMoveDelay -= Time.deltaTime;

        if (Player.HorizontalSpeedMagnitude > LockCamAtHighSpeed && (lookTimer > 0) && !GoingTowardsCamera && afterMoveDelay <= 0)
        {
            //Debug.Log("Move camera behind");
            FollowDirection(3, 14, -10, 0);
        }

        if (Player.HorizontalSpeedMagnitude > 30f && Actions.Action == 12)
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
        if (!Locked || Skip)
        {
            //Debug.Log(Actions.Action);
            //If not grinding or wall running
            if (Actions.Action != 50 || Skip)
            {
                //Debug.Log("Follow Directions" );

                float dot = Vector3.Dot(Skin.forward, transform.right);
                x += (dot * speed) * (Time.deltaTime * 100);

                y = Mathf.Lerp(y, height, Time.deltaTime * Yspeed);
            }
            
        }
    }
    public void FollowDirection(float height, float speed)
    {
        if (!Locked)
        {
            if (Actions.Action != 50)
            {
                //Debug.Log("Follow Directions height");
                if(Player.Grounded)
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
    public void SetCamera(Vector3 dir, float duration, float heightSet, float speed, float lagSet)
    {

        lookAtDir = dir;
        lookTimer = -duration;
        heighttolook = heightSet;
        LockedRotationSpeed = speed;
        moveSpeed = lagSet;
        rotSpeed = lagSet * 0.1f;

    }
    public void SetCamera(Vector3 dir, float duration)
    {

        lookAtDir = dir;
        lookTimer = -duration;
        LockedRotationSpeed = InitialLockedRotationSpeed;

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


    public void GoBehind()
    {

        PlayerPosLerped.position = Target.position;

        if (!Actions.Action05.isZipLine)
        {
            Quaternion newrot = Player.transform.rotation;
            PlayerPosLerped.rotation = Quaternion.Lerp(PlayerPosLerped.rotation, newrot, Time.deltaTime * CameraVerticalRotationSpeed);

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

}




