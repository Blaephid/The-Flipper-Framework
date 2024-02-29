using UnityEngine;
using System.Collections;

public class S_Handler_RingRoad : MonoBehaviour
{

        S_CharacterTools Tools;
        S_PlayerPhysics player;
        public bool HasTarget { get; set; }
        public GameObject TargetObject { get; set; }
        S_ActionManager Actions;
	S_PlayerInput _Input;

        float _targetSearchDistance_ = 10;
        Transform Icon;
        float _iconScale_;

        [HideInInspector] public GameObject[] Targets;
        [HideInInspector] public GameObject[] TgtDebug;

        LayerMask _Layer_;
        Transform MainCamera;


        void Awake () {
                if (Actions == null)
                {
                        Tools = GetComponent<S_CharacterTools>();
                        AssignTools();
                        AssignStats();
                }

        }

        void Start () {

                StartCoroutine(ScanForTargets());
        }


        private void FixedUpdate () {
                if (Actions.whatAction == S_Enums.PrimaryPlayerStates.RingRoad)
                {
                        Collider[] TargetsInRange = GetCloseTargets(_targetSearchDistance_);

                        if (TargetsInRange.Length > 0)
                        {
                                //yield return new WaitForFixedUpdate();
                                TargetObject = GetClosestTarget(TargetsInRange);
                        }
                }
        }

        private IEnumerator ScanForTargets () {
                while (true)
                {
                        yield return new WaitForFixedUpdate();


                        if (Actions.whatAction == S_Enums.PrimaryPlayerStates.Jump || Actions.whatAction == S_Enums.PrimaryPlayerStates.Default)
                        {
                                Collider[] TargetsInRange = GetCloseTargets(_targetSearchDistance_ * 1.45f);

                                if (TargetsInRange.Length > 0)
                                {
                                        yield return new WaitForFixedUpdate();
                                        TargetObject = GetClosestTarget(TargetsInRange);
                                        PerformRingRoad();
                                }

                        }
                        else
                        {
                                yield return new WaitForFixedUpdate();
                        }
                }

        }

        void PerformRingRoad () {
                //Do a LightDash Attack
                if (Actions.whatAction != S_Enums.PrimaryPlayerStates.RingRoad && _Input.InteractPressed && TargetObject != null)
                {
			//Debug.Log("LightDash");
			_Input.CamResetPressed = false;
                        Actions.ChangeAction(S_Enums.PrimaryPlayerStates.RingRoad);
                        Actions.Action07.InitialEvents();
                }
        }

        Collider[] GetCloseTargets ( float maxDistance ) {
                Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, maxDistance, _Layer_, QueryTriggerInteraction.Collide);
                return TargetsInRange;
        }

        GameObject GetClosestTarget ( Collider[] TargetsInRange ) {
                HasTarget = false;

                int checkLimit = 0;
                Transform closestTarget = null;
                foreach (Collider t in TargetsInRange)
                {
                        if (t != null)
                        {
                                Transform target = t.transform;
                                closestTarget = CheckTarget(target, closestTarget);

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

        Transform CheckTarget ( Transform thisTarget, Transform current ) {
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

        void AssignStats () {
                _targetSearchDistance_ = Tools.Stats.RingRoadStats.SearchDistance;
                _iconScale_ = Tools.Stats.RingRoadStats.iconScale;
                _Layer_ = Tools.Stats.RingRoadStats.RingRoadLayer;
        }

        void AssignTools () {
                Actions = GetComponent<S_ActionManager>();
                player = GetComponent<S_PlayerPhysics>();
		_Input = GetComponent<S_PlayerInput>();

                Icon = Tools.homingIcons.GetComponent<Transform>();
                MainCamera = Tools.MainCamera;
        }


}
