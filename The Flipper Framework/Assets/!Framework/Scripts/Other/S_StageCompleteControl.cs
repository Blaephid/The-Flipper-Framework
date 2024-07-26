using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class S_StageCompleteControl : MonoBehaviour {

    //WHERE EVEN IS THIS SCRIPT AND WHY IS IT HERE?


    public float End;
    float counter;
    public int LevelToGoNext;

    public Animator Anim;

    void Update()
    {
        ////Debug.Log("i'm here");
        counter += Time.deltaTime;
        if(counter > End)
        {
            Anim.SetInteger("Action", 1);
            if(counter > End + 2.3f)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SceneManager.LoadScene(LevelToGoNext);
            }
        }
    }

}
