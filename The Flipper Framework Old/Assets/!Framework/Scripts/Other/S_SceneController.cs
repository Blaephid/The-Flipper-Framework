using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class S_SceneController : MonoBehaviour {

    static S_SceneController Instance;
    public Scene Gameplay;
    public static int LevelToLoad;
    public static int LastLoadedLevel;

    S_PlayerPhysics Player;

    void Start()
    {
        if (Instance != null)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        LoadGameplay();
        Player = GameObject.FindWithTag("Player").GetComponent<S_PlayerPhysics>();
        Player.gameObject.SetActive(false);

    }

    void Update()
    {
        
    }

    public static void LoadStageLoading(int StageValue)
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        LevelToLoad = StageValue;
    }

    public static void LoadGameplay()
    {
        SceneManager.LoadScene(2);
    }

    public static void LoadStage(int stage)
    {
        SceneManager.UnloadSceneAsync(LastLoadedLevel);
        SceneManager.LoadSceneAsync(stage, LoadSceneMode.Additive);
        LastLoadedLevel = stage;
    }



}
