using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingAttackControl : MonoBehaviour
{
    CharacterTools tools;

    public bool HasTarget { get; set; }
    [HideInInspector] public GameObject TargetObject;
    GameObject previousTarget;
    
    ActionManager Actions;
    PlayerBhysics player;

    bool scanning = true;
    bool inAir = false;

    float TargetSearchDistance = 10;
    LayerMask TargetLayer;
    LayerMask BlockingLayers;
    float FieldOfView;
    float facingAmount;
    float distance;

    AudioSource IconSound;
    GameObject AlreadyPlayed;
    Animator IconAnim;
    Animator CharacterAnimator;


    Transform Icon;
    float IconScale;
    GameObject normalIcon;
    GameObject damageIcon;

    public static GameObject[] Targets;
    [HideInInspector] public GameObject[] TgtDebug;

    Transform MainCamera;
    float IconDistanceScaling;

    int HomingCount;
    public bool HomingAvailable { get; set; }


    void Awake()
    {
        if (player == null)
        {
            tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }

    }

    void Start()
    {
        //var tgt = GameObject.FindGameObjectsWithTag("HomingTarget");
        //Targets = tgt;
        //TgtDebug = tgt;

        Icon.parent = null;
        StartCoroutine(ScanForTargets(.10f));
    }



    void FixedUpdate()
    {

        //Prevent Homing attack spamming

        HomingCount += 1;

        if (Actions.Action == 2)
        {
            HomingAvailable = false;
            HomingCount = 0;
        }
        if (HomingCount > 3)
        {
            HomingAvailable = true;
        }




        if (HasTarget && TargetObject != null)
        {
            Icon.position = TargetObject.transform.position;
            float camDist = Vector3.Distance(transform.position, MainCamera.position);
            Icon.localScale = (Vector3.one * IconScale) + (Vector3.one * (camDist * IconDistanceScaling));

            if (AlreadyPlayed != TargetObject)
            {
                AlreadyPlayed = TargetObject;
                IconSound.Play();
                IconAnim.SetTrigger("NewTgt");
            }

        }
        else
        {
            Icon.localScale = Vector3.zero;
        }

    }

    IEnumerator ScanForTargets(float secondsBetweenChecks)
    {
        while (scanning)
        {

            while (!player.Grounded && Actions.Action != 5)
            {
                UpdateHomingTargets();
                if (!HasTarget)
                    yield return new WaitForSeconds(secondsBetweenChecks);
                else;
                    yield return new WaitForSeconds(secondsBetweenChecks * 1.5f);
            }
            previousTarget = null;
            HasTarget = false;
            yield return new WaitForSeconds(.1f);
        }


    }

    //This function will look for every possible homing attack target in the whole level. 
    //And you can call it from other scritps via [ HomingAttackControl.UpdateHomingTargets() ]
    public void UpdateHomingTargets()
    {
        HasTarget = false;
        TargetObject = null;
        TargetObject = GetClosestTarget(TargetLayer, TargetSearchDistance, FieldOfView);
        previousTarget = TargetObject;

    }

    public GameObject GetClosestTarget(LayerMask layer, float Radius, float FOV)
    {
        ///First we use a spherecast to get every object with the given layer in range. Then we go through the
        ///available targets from the spherecast to find which is the closest to Sonic.

        GameObject closestTarget = null;
        distance = 0f;

        RaycastHit[] NewTargetsInRange = Physics.SphereCastAll(transform.position, 8f, Camera.main.transform.forward, Radius * 1.5f, layer);
        foreach (RaycastHit t in NewTargetsInRange)
        {
            if (t.collider.gameObject.GetComponent<HomingTarget>())
            {

                Transform target = t.collider.transform;
                closestTarget = checkTarget(target, Radius, closestTarget, 1.5f);
            }
        }
      
        if(closestTarget == null)
        {
            Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, Radius, layer);
            foreach (Collider t in TargetsInRange)
            {

                if (t.gameObject.GetComponent<HomingTarget>())
                {
 
                    Transform target = t.gameObject.transform;
                    closestTarget = checkTarget(target, Radius, closestTarget, 1);
                }

            }

            if (previousTarget != null)
            {
   
                closestTarget = checkTarget(previousTarget.transform, Radius, closestTarget, 1.3f);
            }
        }
        
        return closestTarget;
    }

    GameObject checkTarget(Transform target, float Radius, GameObject closest, float maxDisMod)
    {
        Vector3 Direction = CharacterAnimator.transform.position - target.position;
        float TargetDistance = (Direction.sqrMagnitude / Radius) / Radius;

        if(TargetDistance < maxDisMod * Radius)
        {
            bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < facingAmount; //Make sure Sonic is facing the target enough

            Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position); //Get the target's screen position
            bool onScreen = screenPoint.z > 0.3f && screenPoint.x > 0.08 && screenPoint.x < 0.92f && screenPoint.y > 0f && screenPoint.y < 0.95f; //Make sure the target is on screen

            if ((TargetDistance < distance || distance == 0f) && Facing && onScreen)
            {
                if (!Physics.Linecast(transform.position, target.position, BlockingLayers))
                {
                    HasTarget = true;
                    //Debug.Log(closestTarget);
                    distance = TargetDistance;
                    return target.gameObject;
                }
            }
        }
        
        return closest;
    }

    private void AssignTools()
    {

        Actions = GetComponent<ActionManager>();
        player = GetComponent<PlayerBhysics>();
        CharacterAnimator = tools.CharacterAnimator;

        MainCamera = tools.MainCamera;

        Icon = tools.homingIcons.transform;
        normalIcon = tools.normalIcon;
        damageIcon = tools.weakIcon;

        IconSound = Icon.gameObject.GetComponent<AudioSource>();
        IconAnim = Icon.gameObject.GetComponent<Animator>();
    }

    private void AssignStats()
    {
        TargetSearchDistance = tools.coreStats.TargetSearchDistance;
        TargetLayer = tools.coreStats.TargetLayer;
        BlockingLayers = tools.coreStats.BlockingLayers;
        FieldOfView = tools.coreStats.FieldOfView;
        facingAmount = tools.coreStats.FacingAmount;

        IconScale = tools.coreStats.IconScale;
        IconDistanceScaling = tools.coreStats.IconDistanceScaling;
    }


} 

