using UnityEngine;
using System.Collections;

public class DebugLoadStage : MonoBehaviour {

    public int Stage;

    void Start()
    {
        S_SceneController.LoadStageLoading(3);
    }

}
