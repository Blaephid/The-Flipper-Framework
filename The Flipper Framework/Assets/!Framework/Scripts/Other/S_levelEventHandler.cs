using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using Abertay.Analytics;

public class S_LevelEventHandler : MonoBehaviour
{
    public bool ActivelySendingEvents = false;


    //public AnalyticsManager analyMan;
    Scene scene;

    S_PlayerPhysics playerPhys;
    string curScene;

    float timeInLevel;
    public int hintRingsHit;
    public List<GameObject> hintRings;
    public int Deaths;

    float timeBetween;
    public float thisTime;
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

    public int TotDeaths;

    public int TotJumpsPerformed;
    public int TotRollsPerformed;
    public int TotDoubleJumpsPerformed;
    public int TotSpinChargesPeformed;
    public int TotquickstepsPerformed;
    public int TotBouncesPerformed;
    public int TotdropChargesPerformed;
    public int TotjumpDashesPerformed;
    public int TothomingAttacksPerformed;
    public int TotwallRunsPerformed;
    public int TotwallClimbsPerformed;
    public int TotRailsGrinded;
    public int TotringRoadsPerformed;
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

        timeBetween += Time.deltaTime;
    }

    public IEnumerator logEndEvents()
    {
        LogEvents(false, "END");
        yield return new WaitForFixedUpdate();
        LogEvents(true, "");
    }

    public void LogEvents(bool levelEnd, string checkPoint = "", string levelEvent = "")
    {
        //if(ActivelySendingEvents)
        //{
        //    if(levelEnd)
        //    {
        //        if (scene.name == "TutorialLevel")
        //        {
        //            levelEvent = "Tutorial_Complete";
        //        }
        //        else if (scene.name == "AltitudeLimit")
        //        {
        //            levelEvent = "Altitude_Limit_Complete";
        //        }
        //    }   
        //    else if (checkPoint != "")
        //    {
        //        levelEvent = "Checkpoint_Reached";
        //    }

        //    if (levelEvent != "")
        //    {
        //        increaseTotals();

        //        Dictionary<string, object> parameters;

        //        if(levelEnd)
        //        {

        //            replaceTrackers();

        //            parameters = new Dictionary<string, object>() { { "DeathsPerCheckpoint", deathsPerCheckPoint } };
        //            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //            parameters = new Dictionary<string, object>() { { "Time_to_Beat", timeInLevel } };
        //            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //            parameters = new Dictionary<string, object>() { { "Total_Deaths", Deaths } };
        //            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //            if (levelEvent == "Tutorial_Complete")
        //            {
        //                parameters = new Dictionary<string, object>() { { "HintRingsHit", hintRingsHit } };
        //                AnalyticsManager.SendCustomEvent(levelEvent, parameters);
        //            }
        //        }

        //        else if (checkPoint != "")
        //        {
        //            parameters = new Dictionary<string, object>() { { "Checkpoint_Designation", checkPoint } };
        //            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //            parameters = new Dictionary<string, object>() { { "Time_Between_Checks", timeBetween } };
        //            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //            parameters = new Dictionary<string, object>() { { "Deaths", Deaths } };
        //            AnalyticsManager.SendCustomEvent(levelEvent, parameters);
        //        }

                

        //        parameters = new Dictionary<string, object>() { { "Jumps_Performed", JumpsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "DoubleJumps_Performed", DoubleJumpsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "Rolls_Performed", RollsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "SpinCharges_Performed", SpinChargesPeformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "Quicksteps_Performed", quickstepsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "DropCharges_Performed", dropChargesPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "JumpDashes_Performed", jumpDashesPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "HomingAttacks_Performed", homingAttacksPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "WallRuns_Performed", wallRunsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "WallClimbs_Performed", wallClimbsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "RingRoads_Performed", ringRoadsPerformed } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);

        //        parameters = new Dictionary<string, object>() { { "Rails_Grinded", RailsGrinded } };
        //        AnalyticsManager.SendCustomEvent(levelEvent, parameters);
        //    }

            
        //    ResetTrackers();

        //    if (levelEnd) resetTotalTrackers();
        //}
        
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

    void increaseTotals()
    {
        timeInLevel += timeBetween;
        TotDeaths += Deaths;
        TotJumpsPerformed += JumpsPerformed;
        TotRollsPerformed += RollsPerformed;
        TotDoubleJumpsPerformed += DoubleJumpsPerformed;
        TotSpinChargesPeformed += SpinChargesPeformed;
        TotquickstepsPerformed += quickstepsPerformed;
        TotdropChargesPerformed += dropChargesPerformed;
        TotjumpDashesPerformed += jumpDashesPerformed;
        TothomingAttacksPerformed += homingAttacksPerformed;
        TotwallClimbsPerformed += wallClimbsPerformed;
        TotwallRunsPerformed += wallRunsPerformed;
        TotRailsGrinded += RailsGrinded;
        TotringRoadsPerformed += ringRoadsPerformed;

    }

    void replaceTrackers()
    {
        Deaths = TotDeaths;
        JumpsPerformed = TotJumpsPerformed;
        RollsPerformed = TotRollsPerformed;
        DoubleJumpsPerformed = TotDoubleJumpsPerformed;
        SpinChargesPeformed = TotSpinChargesPeformed;
        quickstepsPerformed = TotquickstepsPerformed;
        dropChargesPerformed = TotdropChargesPerformed;
        jumpDashesPerformed = TotjumpDashesPerformed;
        homingAttacksPerformed = TothomingAttacksPerformed;
        wallClimbsPerformed = TotwallClimbsPerformed;
        wallRunsPerformed = TotwallRunsPerformed;
        RailsGrinded = TotRailsGrinded;
        ringRoadsPerformed = TotringRoadsPerformed;

    }


    public void resetTotalTrackers()
    {
        timeInLevel = 0;
        hintRingsHit = 0;
        Deaths = 0;

        TotJumpsPerformed = 0;
        TotRollsPerformed = 0;
        TotDoubleJumpsPerformed = 0;
        TotSpinChargesPeformed = 0;
        TotquickstepsPerformed = 0;
        TotdropChargesPerformed = 0;
        TotjumpDashesPerformed = 0;
        TothomingAttacksPerformed = 0;
        TotwallRunsPerformed = 0;
        TotwallClimbsPerformed = 0;
        TotRailsGrinded = 0;
        TotringRoadsPerformed = 0;

        deathsPerCheckPoint.Clear();
        currentDeathsPerCP = 0;
    }

    public void ResetTrackers()
    {
        Deaths = 0;
        timeBetween = 0;

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

    }
}
