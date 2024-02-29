using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S_Handler_HomingAttack : MonoBehaviour
{
    S_CharacterTools tools;

    public bool _HasTarget { get; set; }
    [HideInInspector] public GameObject _TargetObject;
    GameObject _PreviousTarget;
    
    S_ActionManager _Actions;
    S_PlayerPhysics _PlayerPhys;

    bool _isScanning = true;

    float _targetSearchDistance_ = 10;
    float _faceRange_ = 66;
    LayerMask _TargetLayer_;
    LayerMask _BlockingLayers_;
    float _facingAmount_;
    float _distance;

    AudioSource _IconSound;
    GameObject _AlreadyPlayed;
    Animator _IconAnim;
    Animator _CharacterAnimator;


    Transform _IconTransform;
    float _iconScale_;
    GameObject _NormalIcon;
    GameObject _DamageIcon;

    public static GameObject[] _ListOfTargets;
    [HideInInspector] public GameObject[] _ListOfTgtDebugs;

    Transform _MainCamera;
    float _iconDistanceScaling_;

    int _homingCount;
    public bool _isHomingAvailable { get; set; }


    void Awake()
    {
        if (_PlayerPhys == null)
        {
            tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();
        }

    }

    void Start()
    {

        _IconTransform.parent = null;
        StartCoroutine(ScanForTargets(.10f));
    }



    void FixedUpdate()
    {

        //Prevent Homing attack spamming

        _homingCount += 1;

        if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Homing)
        {
            _isHomingAvailable = false;
            _homingCount = 0;
        }
        if (_homingCount > 3)
        {
            _isHomingAvailable = true;
        }




        if (_HasTarget && _TargetObject != null)
        {
            _IconTransform.position = _TargetObject.transform.position;
            float camDist = Vector3.Distance(transform.position, _MainCamera.position);
            _IconTransform.localScale = (Vector3.one * _iconScale_) + (Vector3.one * (camDist * _iconDistanceScaling_));

            if (_AlreadyPlayed != _TargetObject)
            {
                _AlreadyPlayed = _TargetObject;
                _IconSound.Play();
                _IconAnim.SetTrigger("NewTgt");
            }

        }
        else
        {
            _IconTransform.localScale = Vector3.zero;
        }

    }

    IEnumerator ScanForTargets(float secondsBetweenChecks)
    {
        while (_isScanning)
        {

            while (!_PlayerPhys._isGrounded && _Actions.whatAction != S_Enums.PrimaryPlayerStates.Rail)
            {
                UpdateHomingTargets();
                if (!_HasTarget)
                    yield return new WaitForSeconds(secondsBetweenChecks);
                else
                {
                    //Debug.Log(Vector3.Distance(transform.position, TargetObject.transform.position));
                    yield return new WaitForSeconds(secondsBetweenChecks * 1.5f);                  
                }
            }
            _PreviousTarget = null;
            _HasTarget = false;
            yield return new WaitForSeconds(.1f);
        }


    }

    //This function will look for every possible homing attack target in the whole level. 
    //And you can call it from other scritps via [ HomingAttackControl.UpdateHomingTargets() ]
    public void UpdateHomingTargets()
    {
        _HasTarget = false;
        _TargetObject = null;
        _TargetObject = GetClosestTarget(_TargetLayer_, _targetSearchDistance_);
        _PreviousTarget = _TargetObject;

    }

    public GameObject GetClosestTarget(LayerMask layer, float Radius)
    {
        ///First we use a spherecast to get every object with the given layer in range. Then we go through the
        ///available targets from the spherecast to find which is the closest to Sonic.

        GameObject closestTarget = null;
        _distance = 0f;
        int checkLimit = 0;
        RaycastHit[] NewTargetsInRange = Physics.SphereCastAll(transform.position, 10f, Camera.main.transform.forward, _faceRange_, layer);
        foreach (RaycastHit t in NewTargetsInRange)
        {
            if (t.collider.gameObject.GetComponent<S_Data_HomingTarget>())
            {

                Transform target = t.collider.transform;
                closestTarget = CheckTarget(target, Radius, closestTarget, 1.5f);
            }

            checkLimit++;
            if (checkLimit > 3)
                break;
        }

        checkLimit = 0;
        if (closestTarget == null)
        {
            Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, Radius, layer);
            foreach (Collider t in TargetsInRange)
            {

                if (t.gameObject.GetComponent<S_Data_HomingTarget>())
                {
 
                    Transform target = t.gameObject.transform;
                    closestTarget = CheckTarget(target, Radius, closestTarget, 1);
                }

                checkLimit++;
                if (checkLimit > 3)
                    break;

            }

            if (_PreviousTarget != null)
            {
   
                closestTarget = CheckTarget(_PreviousTarget.transform, Radius, closestTarget, 1.3f);
            }
        }
        
        return closestTarget;
    }

    GameObject CheckTarget(Transform target, float Radius, GameObject closest, float maxDisMod)
    {
        Vector3 Direction = _CharacterAnimator.transform.position - target.position;
        float TargetDistance = (Direction.sqrMagnitude / Radius) / Radius;

        if(TargetDistance < maxDisMod * Radius)
        {
            bool Facing = Vector3.Dot(_CharacterAnimator.transform.forward, Direction.normalized) < _facingAmount_; //Make sure Sonic is facing the target enough

            Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position); //Get the target's screen position
            bool onScreen = screenPoint.z > 0.3f && screenPoint.x > 0.08 && screenPoint.x < 0.92f && screenPoint.y > 0f && screenPoint.y < 0.95f; //Make sure the target is on screen

            if ((TargetDistance < _distance || _distance == 0f) && Facing && onScreen)
            {
                if (!Physics.Linecast(transform.position, target.position, _BlockingLayers_))
                {
                    _HasTarget = true;
                    //Debug.Log(closestTarget);
                    _distance = TargetDistance;
                    return target.gameObject;
                }
            }
        }
        
        return closest;
    }

    private void AssignTools()
    {

        _Actions = GetComponent<S_ActionManager>();
        _PlayerPhys = GetComponent<S_PlayerPhysics>();
        _CharacterAnimator = tools.CharacterAnimator;

        _MainCamera = tools.MainCamera;

        _IconTransform = tools.homingIcons.transform;
        _NormalIcon = tools.normalIcon;
        _DamageIcon = tools.weakIcon;

        _IconSound = _IconTransform.gameObject.GetComponent<AudioSource>();
        _IconAnim = _IconTransform.gameObject.GetComponent<Animator>();
    }

    private void AssignStats()
    {
        _targetSearchDistance_ = tools.Stats.HomingSearch.targetSearchDistance;
        _faceRange_ = tools.Stats.HomingSearch.faceRange;
        _TargetLayer_ = tools.Stats.HomingSearch.TargetLayer;
        _BlockingLayers_ = tools.Stats.HomingSearch.blockingLayers;
        _facingAmount_ = tools.Stats.HomingSearch.facingAmount;

        _iconScale_ = tools.Stats.HomingSearch.iconScale;
        _iconDistanceScaling_ = tools.Stats.HomingSearch.iconDistanceScaling;
    }


} 

