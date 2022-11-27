using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Abertay.Analytics;

public class levelEventHandler : MonoBehaviour
{
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
        string levelEvent = "";
        if(scene.name == "TutorialLevel")
        {
            levelEvent = "Tutorial Complete";
        }
        else if (scene.name == "AltitudeLimit")
        {
            levelEvent = "Altitude Limit Complete";
        }

        if(levelEvent != "")
        {

            Dictionary<string, object> parameters =
                new Dictionary<string, object>()
                {
                    { "Total Deaths", Deaths }
                };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);


            parameters = new Dictionary<string, object>(){{ "DeathsPerCheckpoint", deathsPerCheckPoint }};
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Time to beat", timeInLevel } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            if (levelEvent == "Tutorial Complete")
            {
                parameters = new Dictionary<string, object>() { { "HintRingsHit", hintRingsHit } };
                AnalyticsManager.SendCustomEvent(levelEvent, parameters);
            }

            parameters = new Dictionary<string, object>() { { "Jumps Performed", JumpsPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Double Jumps Performed", DoubleJumpsPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Rolls Performed", RollsPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Spin Charges Performed", SpinChargesPeformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Quick step Performed", quickstepsPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Drop Charges Performed", dropChargesPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Jump Dashes Performed", jumpDashesPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Homing Attacks Performed", homingAttacksPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Wall Runs Performed", wallRunsPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Wall Climbs Performed", wallClimbsPerformed } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Ring Roads Performed", ringRoadsPerformed} };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);

            parameters = new Dictionary<string, object>() { { "Rails Grinded", RailsGrinded } };
            AnalyticsManager.SendCustomEvent(levelEvent, parameters);
        }

        ResetTrackers();
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
