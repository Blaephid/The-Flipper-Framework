using UnityEngine;
using System.Collections;

public class RingRoadControl : MonoBehaviour {

    CharacterTools Tools;
    PlayerBhysics player;
    public bool HasTarget { get; set; }
    public GameObject TargetObject { get; set; }
    ActionManager Actions;

    float TargetSearchDistance = 10;
    Transform Icon;
    float IconScale;

    [HideInInspector] public GameObject[] Targets;
    [HideInInspector] public GameObject[] TgtDebug;

    LayerMask layer;
    Transform MainCamera;

    bool firstime = false;

    void Awake()
    {
        if (Actions == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();
            AssignStats();
        }
        
    }

    void Start()
    {

        StartCoroutine(ScanForTargets());
    }


    private void FixedUpdate()
    {
        if(Actions.Action == 7)
        {
            Collider[] TargetsInRange = GetCloseTargets(TargetSearchDistance);

            if (TargetsInRange.Length > 0)
            {
                //yield return new WaitForFixedUpdate();
                TargetObject = GetClosestTarget(TargetsInRange);
            }
        }
    }

    private IEnumerator ScanForTargets()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
           

            if (Actions.Action == 1 || Actions.Action == 0)
            {
                Collider[] TargetsInRange = GetCloseTargets(TargetSearchDistance * 1.45f);

                if(TargetsInRange.Length > 0)
                {
                    yield return new WaitForFixedUpdate();
                    TargetObject = GetClosestTarget(TargetsInRange);
                    performRR();
                }

            }
            else
            {
                yield return new WaitForFixedUpdate();
            }
        }
        
    }

    void performRR()
    {
        //Do a LightDash Attack
        if (Actions.Action != 7 && Actions.InteractPressed && TargetObject != null)
        {
            //Debug.Log("LightDash");
            Actions.CamResetPressed = false;
            Actions.ChangeAction(7);
            Actions.Action07.InitialEvents();
        }
    }

    Collider[] GetCloseTargets(float maxDistance)
    {
        Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, maxDistance, layer, QueryTriggerInteraction.Collide);
        return TargetsInRange;
    }

	GameObject GetClosestTarget(Collider[] TargetsInRange)
	{
        HasTarget = false;
      
        int checkLimit = 0;
        Transform closestTarget = null;
        foreach (Collider t in TargetsInRange)
        {
            if (t != null)
            {
                Transform target = t.transform;
                closestTarget = checkTarget(target, closestTarget);

                checkLimit++;
                if (checkLimit > 3)
                    break;
            }

        }

        if (closestTarget != null)
            return closestTarget.gameObject;
        else
            return null;

    }

    Transform checkTarget(Transform thisTarget, Transform current)
    {
        float dis = Vector3.Distance(transform.position, thisTarget.position);

        if (current == null)
            return thisTarget;
        else
        {
            float closDis = Vector3.Distance(transform.position, current.position);
            if (closDis > dis)
            {
                HasTarget = true;
                return thisTarget;
            }

        }
       
        return current;
    }

    void AssignStats()
    {
        TargetSearchDistance = Tools.coreStats.RingTargetSearchDistance;
        IconScale = Tools.coreStats.RingRoadIconScale;
        layer = Tools.coreStats.RingRoadLayer;
    }

    void AssignTools()
    {
        Actions = GetComponent<ActionManager>();
        player = GetComponent<PlayerBhysics>();

        Icon = Tools.homingIcons.GetComponent<Transform>();
        MainCamera = Tools.MainCamera;
    }


}
