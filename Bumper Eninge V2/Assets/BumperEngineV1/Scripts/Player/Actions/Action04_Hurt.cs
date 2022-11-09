using UnityEngine;
using System.Collections;

public class Action04_Hurt : MonoBehaviour {

    Animator CharacterAnimator;
    PlayerBhysics Player;
    CharacterTools Tools;
    ActionManager Actions;
    SonicSoundsControl sounds;

    [HideInInspector] public float KnockbackUpwardsForce = 10;
    int counter;
    public float deadCounter { get; set; }

    [HideInInspector] public bool ResetSpeedOnHit = false;
    [HideInInspector] public float KnockbackForce = 10;

    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();

        }
        
    }

    public void InitialEvents()
    {
        //Change Velocity
        if (!ResetSpeedOnHit)
        {
            Vector3 newSpeed = new Vector3((Player.rb.velocity.x / 2), KnockbackUpwardsForce, (Player.rb.velocity.z / 2));
            newSpeed.y = KnockbackUpwardsForce;
            Player.rb.velocity = newSpeed;
        }
        else
        {
            Vector3 newSpeed = -transform.forward * KnockbackForce;
            newSpeed.y = KnockbackUpwardsForce;
            Player.rb.velocity = newSpeed;
        }

    }

    void FixedUpdate () {

        //Get out of Action
        counter += 1;

        if ((Player.Grounded && counter > 20) || counter > 45)
        {
            if (!Actions.Action04Control.isDead)
            { 
                Actions.ChangeAction(0);
                //Debug.Log("What");
                counter = 0;
            }
        }

    }

    void Update()
    {

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 4);

        //Dead
        if (Actions.Action04Control.isDead)
        {
            deadCounter += Time.deltaTime;
            if (Player.Grounded && deadCounter > 0.3f)
            {
                CharacterAnimator.SetBool("Dead", true);
            }
        }

    }

    private void AssignStats ()
    {
        KnockbackForce = Tools.stats.KnockbackForce;
        KnockbackUpwardsForce = Tools.stats.KnockbackUpwardsForce;
        ResetSpeedOnHit = Tools.stats.ResetSpeedOnHit;
    }

    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
    }
}
