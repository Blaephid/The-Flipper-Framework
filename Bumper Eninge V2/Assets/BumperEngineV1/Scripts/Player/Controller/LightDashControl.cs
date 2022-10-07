using UnityEngine;
using System.Collections;

public class LightDashControl : MonoBehaviour {

    CharacterTools Tools;
    CharacterStats Stats;

    public bool HasTarget { get; set; }
    public static GameObject TargetObject { get; set; }
    ActionManager Actions;

    float TargetSearchDistance = 10;
    Transform Icon;
    float IconScale;

    [HideInInspector] public GameObject[] Targets;
    [HideInInspector] public GameObject[] TgtDebug;

    Transform MainCamera;

    bool firstime = false;

    void Awake()
    {
        if (Actions == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            Stats=GetComponent<CharacterStats>();
            AssignStats();
        }
        
    }

    void Start()
    {
        var tgt = GameObject.FindGameObjectsWithTag("Ring");
        Targets = tgt;
        TgtDebug = tgt;

        Icon.parent = null;
        UpdateHomingTargets();
    }

    void LateUpdate()
    {
        if (!firstime)
        {
            firstime = true;
            UpdateHomingTargets();
        }
    }

    void FixedUpdate()
    {

        UpdateHomingTargets();
        //Prevent Homing attack spamming

        TargetObject = GetClosestTarget(Targets, TargetSearchDistance);

    }

    //This function will look for every possible homing attack target in the whole level. 
    //And you can call it from other scritps via [ HomingAttackControl.UpdateHomingTargets() ]
    public void UpdateHomingTargets()
    {
		var tgt = GameObject.FindGameObjectsWithTag("Ring");
        Targets = tgt;
    }

	GameObject GetClosestTarget(GameObject[] tgts, float maxDistance)
	{
		HasTarget = false;
		GameObject[] gos = tgts;
		GameObject closest = null;
		float distance = maxDistance;
		Vector3 position = transform.position;
		foreach (GameObject go in gos)
		{
			Vector3 diff = go.transform.position - position;
			float curDistance = diff.sqrMagnitude;

			Vector3 screenPoint = MainCamera.GetComponent<Camera>().WorldToViewportPoint(go.transform.position);
			bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

			if (curDistance < distance && onScreen) {
			//	//Debug.Log ("Hitting Homing Target");
				HasTarget = true;
				closest = go;
				distance = curDistance;
				//AimBall.gameObject.SetActive (true);
				//AimBall.position = go.transform.position;
			} 
		}
		////Debug.Log(closest);
		return closest;
	}

    void AssignStats()
    {
        TargetSearchDistance = Stats.LightDashTargetSearchDistance;
        IconScale = Stats.LightDashIconScale;
    }

    void AssignTools()
    {
        Actions = GetComponent<ActionManager>();

        Icon = Tools.homingIcons.GetComponent<Transform>();
        MainCamera = Tools.MainCamera;
    }


}
