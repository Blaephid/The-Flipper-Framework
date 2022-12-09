using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Abertay.Analytics;

public class levelEventHandler : MonoBehaviour
{
    public bool ActivelySendingEvents = false;


    public AnalyticsManager analyMan;
    Scene scene;

    PlayerBhysics playerPhys;
    string curScene;

    float timeInLevel;
    public int hintRingsHit;
    public List<GameObject> hintRings;
    public int Deaths;

    public int JumpsPerformed;
    public int RollsPerformed;
    public int DoubleJumpsPerformed;
    public int SpinChargesPeformed;
    public int quickstepsPerformed;
    public int BouncesPerformed;
    public int dropChargesPerformed;
    public int jumpDashesPerformed;
    public int homingAttacksPerformed;
    public int wallRunsPerformed;
    public int wallClimbsPerformed;
    public int RailsGrinded;
    public int ringRoadsPerformed;

    public List<int> deathsPerCheckPoint;
    public int currentDeathsPerCP;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!ActivelySendingEvents)
            this.gameObject.SetActive(false);

        scene = SceneManager.GetActiveScene();

        if (scene.name != curScene)
        {
            ResetTrackers();
            curScene = scene.name;
        }

        timeInLevel += Time.deltaTime;
    }

    public void LogEvents()
    {
        if(ActivelySendingEvents)
        {
            string levelEvent = "";
            if (scene.name == "TutorialLevel")
            {
                levelEvent = "Tutorial_Complete";
            }
            else if (scene.name == "AltitudeLimit")
            {
                levelEvent = "Altitude_Limit_Complete";
            }

            if (levelEvent != "")
            {

                Dictionary<string, object> parameters =
                    new Dictionary<string, object>()
                    {
                    { "Total_Deaths", Deaths }
                    };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);


                parameters = new Dictionary<string, object>() { { "DeathsPerCheckpoint", deathsPerCheckPoint } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "Time_to_beat", timeInLevel } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                if (levelEvent == "Tutorial_Complete")
                {
                    parameters = new Dictionary<string, object>() { { "HintRingsHit", hintRingsHit } };
                    AnalyticsManager.SendCustomEvent(levelEvent, parameters);
                }

                parameters = new Dictionary<string, object>() { { "Jumps_Performed", JumpsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "DoubleJumps_Performed", DoubleJumpsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "Rolls_Performed", RollsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "SpinCharges_Performed", SpinChargesPeformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "Quicksteps_Performed", quickstepsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "DropCharges_Performed", dropChargesPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "JumpDashes_Performed", jumpDashesPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "HomingAttacks_Performed", homingAttacksPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "WallRuns_Performed", wallRunsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "WallClimbs_Performed", wallClimbsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "RingRoads_Performed", ringRoadsPerformed } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);

                parameters = new Dictionary<string, object>() { { "Rails_Grinded", RailsGrinded } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);
            }

            ResetTrackers();
        }
        
    }

    public void Death()
    {
        Deaths += 1;
        currentDeathsPerCP += 1;
    }

    public void AddDeathsPerCP()
    {
        deathsPerCheckPoint.Add(currentDeathsPerCP);
        currentDeathsPerCP = 0;
    }

    public void ResetTrackers()
    {
        timeInLevel = 0;
        hintRingsHit = 0;
        Deaths = 0;

        JumpsPerformed = 0;
        RollsPerformed = 0;
        DoubleJumpsPerformed = 0;
        SpinChargesPeformed = 0;
        quickstepsPerformed = 0;
        dropChargesPerformed = 0;
        jumpDashesPerformed = 0;
        homingAttacksPerformed = 0;
        wallRunsPerformed = 0;
        wallClimbsPerformed = 0;
        RailsGrinded = 0;
        ringRoadsPerformed = 0;

        deathsPerCheckPoint.Clear();
        currentDeathsPerCP = 0;
    }
}
