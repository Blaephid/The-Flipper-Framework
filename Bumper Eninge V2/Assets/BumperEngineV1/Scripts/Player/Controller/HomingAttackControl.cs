using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingAttackControl : MonoBehaviour
{
    CharacterTools tools;
    CharacterStats stats;

    public bool HasTarget { get; set; }
    [HideInInspector] public GameObject TargetObject;
    
    ActionManager Actions;
    PlayerBhysics player;

    bool scanning = true;
    bool inAir = false;

    float TargetSearchDistance = 10;
    LayerMask TargetLayer;
    LayerMask BlockingLayers;
    float FieldOfView;
    float facingAmount;

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

    bool firstime = false;

    void Awake()
    {
        if (player == null)
        {
            tools = GetComponent<CharacterTools>();
            AssignTools();

            stats = GetComponent<CharacterStats>();
            AssignStats();
        }

    }

    void Start()
    {
        //var tgt = GameObject.FindGameObjectsWithTag("HomingTarget");
        //Targets = tgt;
        //TgtDebug = tgt;

        Icon.parent = null;
        StartCoroutine(ScanForTargets(.12f));
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
                yield return new WaitForSeconds(secondsBetweenChecks);
            }
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


    }

    public GameObject GetClosestTarget(LayerMask layer, float Radius, float FOV)
    {
        ///First we use a spherecast to get every object with the given layer in range. Then we go through the
        ///available targets from the spherecast to find which is the closest to Sonic.
        RaycastHit[] TargetsInRange = Physics.SphereCastAll(transform.position, Radius, transform.forward, Radius, layer);
        GameObject closestTarget = null;
        float distance = 0f;
        //Debug.Log(TargetsInRange.Length);

        foreach (RaycastHit t in TargetsInRange)
        {

            //Debug.Log(t.transform.gameObject);
            if (t.collider.gameObject.GetComponent<HomingTarget>())
            {
                Transform target = t.collider.gameObject.transform;

                Vector3 Direction = CharacterAnimator.transform.position - target.position;
                //Vector3 forward = CharacterAnimator.transform.TransformDirection(Vector3.forward);
                //Debug.Log(Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized));
                bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < facingAmount; //Make sure Sonic is facing the target enough
                //Facing = true;
                float TargetDistance = (Direction.sqrMagnitude / Radius) / Radius;
                //float TargetDistance = t.distance;
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position); //Get the target's screen position
                bool onScreen = screenPoint.z > 0.3f && screenPoint.x > 0.1 && screenPoint.x < 0.9f && screenPoint.y > 0.2f && screenPoint.y < 1f; //Make sure the target is on screen

                //Debug.Log(TargetDistance);
                //Debug.Log(distance);
                //Debug.Log(Facing);
                //Debug.Log(onScreen);


                if ((TargetDistance < distance || distance == 0f) && Facing && onScreen)
                {
                    if (!Physics.Linecast(transform.position, target.position, BlockingLayers))
                    {
                        HasTarget = true;
                        closestTarget = t.collider.gameObject;
                        //Debug.Log(closestTarget);
                        distance = TargetDistance;
                    }
                }
            }

        }
        return closestTarget;
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
        TargetSearchDistance = stats.TargetSearchDistance;
        TargetLayer = stats.TargetLayer;
        BlockingLayers = stats.BlockingLayers;
        FieldOfView = stats.FieldOfView;
        facingAmount = stats.FacingAmount;

        IconScale = stats.IconScale;
        IconDistanceScaling = stats.IconDistanceScaling;
    }


} 

