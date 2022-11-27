using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenControl : MonoBehaviour {

    public Image BlackFade;
    public Animator PressStartAnim;
    public float PressStartIncreaceSpeed;    
    public float LogoRotationAccell;
    public float EndStageAt;
    public AudioSource Source;
    bool End = false;
    float counter;


    void Start()
    {
        BlackFade.enabled = true;
    }

    void Update () {

        if (End)
        {
            counter += Time.deltaTime;            
            PressStartAnim.speed = PressStartIncreaceSpeed;
            if (counter > EndStageAt)
            {
                BlackFade.color = Color.Lerp(BlackFade.color, Color.black, Time.deltaTime * 8);
                if(counter > EndStageAt + 1.5f)
                {
                    SceneManager.LoadScene("CharacterSelect");
                }
            }
        }
        else
        {
            Color a = Color.black;
            a.a = 0;
            BlackFade.color = Color.Lerp(BlackFade.color, a, Time.deltaTime * 3);

        }

        if(Input.GetButtonDown("Start") || Input.GetButtonDown("A"))
        {
            if (!End)
            {
                Source.Play();
                End = true;
            }
        }

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit ();
		}

    }
}
