
using UnityEngine;
using System.Collections;
using Cinemachine;
using SplineMesh;


[RequireComponent(typeof(Spline))]
public class Pathers_Interaction : MonoBehaviour
{
    CharacterTools Tools;
    //The Bare Minimum

    Collider playerCol;
   
    PlayerBhysics Player;
    Animator CharacterAnimator;
    PlayerBinput Input;
    ActionManager Actions;
    Collider CurentPathTrigger;
    [HideInInspector] public CineStart currentCam;

    //Spline        
    public float DistanceIncrements = 0.05f;

    float ClosestSample = 0f;
    public Spline RailSpline { get; set; }
    public CurveSample RailSample { get; set; }


    Transform CollisionPoint;
    Vector3 pointSum;
    float CurrentDist, dist;

    //Upreel
    PulleyActor currentUpreel;
    Transform HandGripPoint;
    [HideInInspector] public Transform feetPoint;

    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();
        }

    }


    private void Update()
    {

        //Updates if the player is currently on an Upreel
        if (currentUpreel != null)
        {
            //Deactives player control and freezes movemnt to keep them in line with the upreel.

            Player.rb.velocity = Vector3.zero;
            Input.LockInputForAWhile(0f, true);

            //Moves the player to the position of the Upreel
            Vector3 HandPos = transform.position - HandGripPoint.position;
            transform.position = currentUpreel.HandleGripPos.position + HandPos;
            transform.forward = -currentUpreel.transform.forward;

            //If the upreel is finished
            if (!currentUpreel.Moving)
            {
                Input.LockInputForAWhile(20f, true);

                currentUpreel = null;
                Actions.Action02.HomingAvailable = true;

                //Bounces player up to get them above the wall
                StartCoroutine(exitPulley());

            }
        }

    }

    IEnumerator exitPulley()
    {
        Player.rb.velocity = Vector3.zero;
        Player.AddVelocity(new Vector3(0f, 60f, 0f));

        yield return new WaitForSeconds(.2f);

        //Actions.ChangeAction(0);
        Player.AddVelocity(CharacterAnimator.transform.forward * 15f);
    }

    public void OnCollisionEnter(Collision col)
    {

        //Because Rails are made of mesh colliders, they can't be triggers, so interacting with them must be with an OnCollisionEnter
        
        if (col.gameObject.CompareTag("Rail"))
        {
            AttachToRail(col);           
        }
    }

    public void AttachToRail(Collision col)
    {
        //Debug.Log("HitRail");
        if (col.gameObject.GetComponentInParent<Spline>())
        {
            //Debug.Log("Rail!");
            RailSpline = col.gameObject.GetComponentInParent<Spline>();
            Transform ColPos = GetCollisionPoint(col);
            float Range = GetClosestPos(ColPos);

            if (!Actions.Action05.OnRail && !Actions.Action04Control.isDead)
            {
                //Sets the player to the rail grind action, and sets their position and what spline to follow.
                Actions.Action05.InitialEvents(Range, RailSpline.transform, false);
                Actions.ChangeAction(5);
            }
        }
    }


    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("ZipLine"))
        {
  
            if (col.transform.GetComponent<PulleyObject>())
            {
                    
                RailSpline = col.transform.GetComponent<PulleyObject>().Rail;

                if (!Actions.Action05.OnRail)
                {

                    //Snaps player to the pulley
                    Rigidbody zipbody = col.GetComponent<Rigidbody>();
                    Actions.Action05.pulley = col.transform;
                    Actions.Action05.ZipBody = zipbody;
                    zipbody.isKinematic = false;
                    float Range = GetClosestPos(col.transform);

                    //Sets the player to the rail grind action, and sets their position and what spline to follow.
                    Actions.Action05.InitialEvents(Range, RailSpline.transform, true);
                    Actions.ChangeAction(5);
                }
            }
        }

        //Upreels
        if (col.gameObject.CompareTag("PulleyHandle"))
        {
            //Activates the upreel to start retracting. See PulleyActor class for more.
            //Sets currentUpreel. See Update() above for more.
            currentUpreel = col.gameObject.GetComponentInParent<PulleyActor>();
            currentUpreel.RetractPulley();
            Actions.ChangeAction(0);
        }

        //Automatic Paths
        if (col.gameObject.CompareTag("PathTrigger"))
        {

            //If the player is already on a path, then hitting this trigger will end it.
            if (Actions.Action == 10)
            {
                //See MoveAlongPath for more
                Actions.Action10.ExitPath();
           
                return;
                
            }

            
            //If the player is entering a trigger to start a path.
            if (Actions.Action != 10 && col.gameObject.name != "End")
            {
                float speedGo = 0f;

                //If the path is being started by a path speed pad
                if (col.gameObject.GetComponent<SpeedPadData>())
                {
                    RailSpline = col.gameObject.GetComponent<SpeedPadData>().path;
                    col.GetComponent<AudioSource>().Play();
                    speedGo = col.gameObject.GetComponent<SpeedPadData>().Speed;
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
                    if (col.gameObject.GetComponent<CineStart>())
                    {
                        currentCam = col.gameObject.GetComponent<CineStart>();
                        currentCam.ActivateCam();
                    }

                    CurentPathTrigger = col.GetComponent<Collider>();



                    //Physics.IgnoreCollision(CurentPathTrigger, playerCol, true);


                    //Starts the player moving along the path using the path follow action
                    Actions.Action10.InitialEvents(range, RailSpline.transform, back, speedGo);
                    Actions.ChangeAction(10);

                   
                } 
            }
            
        }

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


    float GetClosestPos(Transform ColPos)
    {
        //spline.nodes.Count - 1

        //Debug.Log(ColPos.position);
        CurrentDist = 9999999f;
        for (float n = 0; n < RailSpline.Length; n += 3)
        {
            dist = ((RailSpline.GetSampleAtDistance(n).location + RailSpline.transform.position) - ColPos.position).sqrMagnitude;
            if (dist < CurrentDist)
            {
                CurrentDist = dist;
                ClosestSample = n;
            }

        }
        return ClosestSample;
    }

    //Called when leaving a pulley to prevent player attaching to it immediately.

    public IEnumerator JumpFromPulley(Transform pulley)
    {
        pulley.GetComponent<CapsuleCollider>().enabled = false;
        GameObject target = pulley.GetComponentInChildren<HomingTarget>().gameObject;
        target.SetActive(false);

        yield return new WaitForSeconds(2f);

        pulley.GetComponent<CapsuleCollider>().enabled = true;
        target.SetActive(true);

    }

    private void AssignTools()
    {
        Actions = GetComponent<ActionManager>();
        Player = GetComponent<PlayerBhysics>();
        Input = GetComponent<PlayerBinput>();

        CharacterAnimator = Tools.CharacterAnimator;
        playerCol = Tools.characterCapsule.GetComponent<Collider>();
        HandGripPoint = Tools.HandGripPoint;
        feetPoint = Tools.FeetPoint;
    }
}

    

