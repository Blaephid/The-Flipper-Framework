using UnityEngine;
using System.Collections;
using SplineMesh;

public class ActionManager : MonoBehaviour
{


    public int Action { get; set; }
    public int PreviousAction { get; set; }

    //Action Scrips, Always leave them in the correct order;

    public Action00_Regular Action00;
    public Action01_Jump Action01;
    public Action02_Homing Action02;
    public Action03_SpinDash Action03;
    public HomingAttackControl Action02Control;
    public Action04_Hurt Action04;
    public HurtControl Action04Control;
    public Action05_Rail Action05;
    public Action06_Bounce Action06;
    public Action07_LightDash Action07;
    public LightDashControl Action07Control;
    public Action08_DropDash Action08;

    //Etc

    PlayerBhysics Phys;

    void Start()
    {
        Phys = GetComponent<PlayerBhysics>();
        ChangeAction(0);
    }

    void DeactivateAllActions()
    {
        //Put all actions here
        //Also put variables that you want re-set out of actions
        if (Action00 != null)
        {
            Action00.enabled = false;
        }
        if (Action01 != null)
        {
            Action01.enabled = false;
        }
        if (Action02 != null)
        {
            Action02.enabled = false;
            Action02.ResetHomingVariables();
        }
        if (Action03 != null)
        {
            Action03.enabled = false;
            Action03.ResetSpinDashVariables();
        }
        if (Action04 != null)
        {
            Action04.enabled = false;
        }
        if (Action05 != null)
        {
            Action05.enabled = false;
        }
        if (Action06 != null)
        {
            Action06.enabled = false;
        }
        if (Action07 != null)
        {
            Action07.enabled = false;
        }
        if (Action08 != null)
        {
            Action08.enabled = false;
        }

    }

    //Call this function to change the action

    public void ChangeAction(int ActionToChange)
    {
        PreviousAction = Action;
        Action = ActionToChange;
        DeactivateAllActions();

        //Put an case for all your actions here
        switch (ActionToChange)
        {
            case 0:
                Action00.enabled = true;
                break;
            case 1:
                Action01.enabled = true;
                break;
            case 2:
                Action02.enabled = true;
                break;
            case 3:
                Action03.enabled = true;
                break;
            case 4:
                Action04.enabled = true;
                break;
            case 5:
                Action05.enabled = true;
                break;
            case 6:
                Action06.enabled = true;
                break;
            case 7:
                Action07.enabled = true;
                break;
            case 8:
                Action08.enabled = true;
                break;
            default:
                //Debug.Log("Action is not available.");
                break;
        }

    }

}
