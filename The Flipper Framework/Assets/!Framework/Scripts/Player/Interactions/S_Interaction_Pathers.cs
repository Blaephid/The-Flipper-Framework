
using UnityEngine;
using System.Collections;
using Cinemachine;
using SplineMesh;


//[RequireComponent(typeof(Spline))]
public class S_Interaction_Pathers : MonoBehaviour
{
    S_CharacterTools Tools;
    //The Bare Minimum

    Collider playerCol;
   
    S_PlayerPhysics Player;
    Animator CharacterAnimator;
    S_PlayerInput Input;
    S_ActionManager Actions;
    Collider CurentPathTrigger;
    [HideInInspector] public S_Trigger_CineCamera currentCam;

    //Spline        
    public float DistanceIncrements = 0.05f;

    float ClosestSample = 0f;
    public Spline RailSpline { get; set; }
    public CurveSample RailSample { get; set; }


    Transform CollisionPoint;
    Vector3 pointSum;
    float CurrentDist, dist;

    //Upreel
    [HideInInspector] public S_Upreel currentUpreel;
    Transform HandGripPoint;
    [HideInInspector] public Transform feetPoint;
    float _offsetUpreel_ = 1.5f;

    bool onceThisFrame = false;

    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            _offsetUpreel_ = Tools.Stats.RailPosition.upreel;
        }

    }


    private void Update()
    {

        //Updates if the player is currently on an Upreel
        if (currentUpreel != null)
        {
            onUpreel();
        }

    }

    private void FixedUpdate()
    {

    }

    void onUpreel()
    {
        
        //If the upreel is finished
        if (!currentUpreel.Moving)
        {
            Input.LockInputForAWhile(20f, false);

            currentUpreel = null;
            Actions.Action02.HomingAvailable = true;

            //CharacterAnimator.transform.rotation = Quaternion.LookRotation(-currentUpreel.transform.forward, transform.up);
            CharacterAnimator.SetInteger("Action", 0);

            //Bounces player up to get them above the wall
            StartCoroutine(exitPulley());

        }
        else
        {
            //Deactives player control and freezes movemnt to keep them in line with the upreel.

            Player._RB.velocity = Vector3.zero;
            Input.LockInputForAWhile(0f, false);

            //Moves the player to the position of the Upreel
            Vector3 HandPos = transform.position - HandGripPoint.position;
            HandPos += (currentUpreel.transform.forward * _offsetUpreel_);
            transform.position = currentUpreel.HandleGripPos.position + HandPos;
            CharacterAnimator.transform.rotation = Quaternion.LookRotation(-currentUpreel.transform.forward, currentUpreel.transform.up);
        }
    }

    IEnumerator exitPulley()
    {
        Player._RB.velocity = Vector3.zero;
        Player.AddCoreVelocity(new Vector3(0f, 60f, 0f));

        yield return new WaitForSeconds(.2f);

        //Actions.ChangeAction(0);
        Player.AddCoreVelocity(CharacterAnimator.transform.forward * 15f);
    }

    public void OnCollisionEnter(Collision col)
    {

        //Because Rails are made of mesh colliders, they can't be triggers, so interacting with them must be with an OnCollisionEnter
        
        if (col.gameObject.CompareTag("Rail"))
        {
           // AttachToRail(col, false, col.collider);           
        }
    }

    public void AttachToRail(Collision col, bool trigger, Collider colli)
    {

        
        if (colli.gameObject.GetComponentInParent<Spline>())
        {

            RailSpline = colli.gameObject.GetComponentInParent<Spline>();
            Transform ColPos;
            if (trigger)
            {
                ColPos = transform;
            }
            else
                ColPos = GetCollisionPoint(col);

            float Range = GetClosestPos(ColPos.position, RailSpline);

            Vector3 offSet = Vector3.zero;

            if(colli.gameObject.GetComponentInParent<S_PlaceOnSpline>())
            {
                offSet.x = colli.gameObject.GetComponentInParent<S_PlaceOnSpline>().Offset3d.x;
            }

            S_AddOnRail addOn = null;

            if(colli.gameObject.GetComponentInParent<S_AddOnRail>())
            {
                if(colli.gameObject.GetComponentInParent<S_AddOnRail>() != null)
                {
                    addOn = colli.gameObject.GetComponentInParent<S_AddOnRail>();
 
                }
            }

            if (!Actions.Action04Control.isDead)
            {
                //Sets the player to the rail grind action, and sets their position and what spline to follow.
                Actions.Action05.InitialEvents(Range, RailSpline.transform.parent, false, offSet, addOn);
                Actions.ChangeAction(S_Enums.PlayerStates.Rail);
            }
        }
    }


    public void OnTriggerEnter(Collider col)
    {

        if (col.gameObject.CompareTag("Rail") && !Actions.Action05.OnRail)
        {

            if(col.GetComponent<CapsuleCollider>().radius == 4)
            {
                if(Player._speedMagnitude > 120 || Mathf.Abs(Player._RB.velocity.y) > 30)
                {
                    AttachToRail(null, true, col);
                }
            }
            else if (col.GetComponent<CapsuleCollider>().radius == 3)
            {
                if (Player._speedMagnitude > 80 || Mathf.Abs(Player._RB.velocity.y) > 20)
                {
                    AttachToRail(null, true, col);
                }
            }
            else if (col.GetComponent<CapsuleCollider>().radius == 2)
            {
                if (Player._speedMagnitude > 40 || Mathf.Abs(Player._RB.velocity.y) > 10)
                {
                    AttachToRail(null, true, col);
                }
            }
            else
            {
                if (col.GetComponent<CapsuleCollider>().radius == 1)
                {
                    AttachToRail(null, true, col);
                }
            }
        }


        if (col.gameObject.CompareTag("ZipLine"))
        {
  
            if (col.transform.GetComponent<S_Control_PulleyObject>())
            {
                    
                RailSpline = col.transform.GetComponent<S_Control_PulleyObject>().Rail;

                if (!Actions.Action05.OnRail)
                {

                    //Snaps player to the pulley
                    Rigidbody zipbody = col.GetComponent<Rigidbody>();
                    Actions.Action05.ZipHandle = col.transform;
                    Actions.Action05.ZipBody = zipbody;
                    zipbody.isKinematic = false;
                    float Range = GetClosestPos(col.transform.position, RailSpline);

                    GameObject target = col.transform.GetComponent<S_Control_PulleyObject>().homingtgt;
                    target.SetActive(false);

                    //Sets the player to the rail grind action, and sets their position and what spline to follow.
                    Actions.Action05.InitialEvents(Range, RailSpline.transform, true, Vector3.zero, null);
                    Actions.ChangeAction(S_Enums.PlayerStates.Rail);
                }
            }
        }

        //Upreels
        if (col.gameObject.CompareTag("PulleyHandle"))
        {
            //Activates the upreel to start retracting. See PulleyActor class for more.
            //Sets currentUpreel. See Update() above for more.
            currentUpreel = col.gameObject.GetComponentInParent<S_Upreel>();

            CharacterAnimator.SetInteger("Action", 9);
            CharacterAnimator.SetTrigger("HitRail");

            currentUpreel.RetractPulley();
            Actions.ChangeAction(S_Enums.PlayerStates.Regular);
        }

        //Automatic Paths
        if (col.gameObject.CompareTag("PathTrigger"))
        {
            if(!onceThisFrame)
                StartCoroutine(SetOnPath(col));           
        }

    }

    private void OnTriggerExit(Collider col)
    {
        //Automatic Paths
        if (col.gameObject.CompareTag("PathTrigger"))
        {
            onceThisFrame = false;
        }
    }

    IEnumerator SetOnPath(Collider col)
    {
        onceThisFrame = true;

        //If the player is already on a path, then hitting this trigger will end it.
        if (Actions.whatAction == S_Enums.PlayerStates.Path || col.gameObject.name == "End")
        {
            //See MoveAlongPath for more
            Actions.Action10.ExitPath();

        }

        else
        {
            float speedGo = 0f;

            //If the path is being started by a path speed pad
            if (col.gameObject.GetComponent<S_Data_SpeedPad>())
            {
                Debug.Log("Enter Path Spline from Pad");
                RailSpline = col.gameObject.GetComponent<S_Data_SpeedPad>().path;
                //col.GetComponent<AudioSource>().Play();
                speedGo = Mathf.Max(col.gameObject.GetComponent<S_Data_SpeedPad>().Speed, Player._horizontalSpeedMagnitude);
            }

            //If the path is being started by a normal trigger
            else if (col.gameObject.GetComponentInParent<Spline>() && col.gameObject.CompareTag("PathTrigger"))
                RailSpline = col.gameObject.GetComponentInParent<Spline>();
            else
                RailSpline = null;

            //If the player has been given a path to follow. This cuts out speed pads that don't have attached paths.
            if (RailSpline != null)
            {
                //noDelay, the coroutine and otherCol. enabled are used to prevent the player colliding with the trigger multiple times for all of their attached colliders

                //Sets the player to start at the start and move forwards
                bool back = false;
                float range = 0f;

                //If entering an Exit trigger, the player will be set to move backwards and start at the end.
                if (col.gameObject.name == "Exit")
                {
                    back = true;
                    range = RailSpline.Length - 1f;
                }

                //If the paths has a set camera angle.
                if (col.gameObject.GetComponent<S_Trigger_CineCamera>())
                {
                    currentCam = col.gameObject.GetComponent<S_Trigger_CineCamera>();
                    currentCam.ActivateCam(8f);
                }

                CurentPathTrigger = col.GetComponent<Collider>();

                //Physics.IgnoreCollision(CurentPathTrigger, playerCol, true);


                //Starts the player moving along the path using the path follow action
                Actions.Action10.InitialEvents(range, RailSpline.transform, back, speedGo);
                Actions.ChangeAction(S_Enums.PlayerStates.Path);
            }
        }
        yield return new WaitForEndOfFrame();
        
    }


    public Transform GetCollisionPoint(Collision col)
    {
        CollisionPoint = transform;
        foreach (ContactPoint contact in col.contacts)
        {
            //Set Middle Point
            pointSum = Vector3.zero;
            for (int i = 0; i < col.contacts.Length; i++)
            {
                pointSum = pointSum + col.contacts[i].point;
            }
            pointSum = pointSum / col.contacts.Length;
            CollisionPoint.position = pointSum;
        }
        return CollisionPoint;
    }


    public float GetClosestPos(Vector3 ColPos, Spline thisSpline)
    {
        //spline.nodes.Count - 1

        //Debug.Log(ColPos.position);
        CurrentDist = 9999999f;
        for (float n = 0; n < thisSpline.Length; n += 3)
        {
            dist = ((thisSpline.GetSampleAtDistance(n).location + thisSpline.transform.position) - ColPos).sqrMagnitude;
            if (dist < CurrentDist)
            {
                CurrentDist = dist;
                ClosestSample = n;
            }

        }
        return ClosestSample;
    }

    //Called when leaving a pulley to prevent player attaching to it immediately.

    public IEnumerator JumpFromZipLine(Transform zipHandle, float time)
    {
        zipHandle.GetComponent<CapsuleCollider>().enabled = false;
        GameObject target = zipHandle.transform.GetComponent<S_Control_PulleyObject>().homingtgt;
        target.SetActive(false);

        yield return new WaitForSeconds(time);

        zipHandle.GetComponent<CapsuleCollider>().enabled = true;
        target.SetActive(true);
        zipHandle.GetComponentInChildren<MeshCollider>().enabled = true;

    }

    private void AssignTools()
    {
        Actions = GetComponent<S_ActionManager>();
        Player = GetComponent<S_PlayerPhysics>();
        Input = GetComponent<S_PlayerInput>();

        CharacterAnimator = Tools.CharacterAnimator;
        playerCol = Tools.characterCapsule.GetComponent<Collider>();
        HandGripPoint = Tools.HandGripPoint;
        feetPoint = Tools.FeetPoint;
    }
}

    

