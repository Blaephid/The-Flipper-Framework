using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action09_SpinKick : MonoBehaviour
{
    CharacterTools Tools;

    ActionManager Actions;
    PlayerBhysics Player;
    PlayerBinput Input;

    Animator CharacterAnimator;
    SonicSoundsControl Sounds;
    SonicEffectsControl Effects;

    CapsuleCollider CoreCol;
    CapsuleCollider slideCol;
    public GameObject KickCollider;
    float counter;
    float KickDuration = 2f;
    float kickForce = 20f;
    float kickDamage = 1f;

    float originalTurnSpeed;
    float originalAccel;
    float originalDecel;
    Vector2 originalDirection;



    // Start is called before the first frame update
 

    private void Awake()
    {
        if (Player == null)
        {
            KickCollider.SetActive(false);

            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }
        
    }

    public void InitialEvents()
    {
        KickCollider.SetActive(true);
        

        counter = KickDuration;

        //Saves speeds to return to after finishing the move.
        originalTurnSpeed = Player.TurnSpeed;
        originalAccel = Player.MoveAccell;
        originalDecel = Player.MoveDecell;
        originalDirection = new Vector2(Player.rb.velocity.x, Player.rb.velocity.z);

        //CHanges collider to decrease height
        slideCol.gameObject.SetActive(true);
        CoreCol.gameObject.SetActive(false);

        //Temporarily locks movement
        if (Player.SpeedMagnitude >= 40f)
        {
            Input = GetComponent<PlayerBinput>();
            Input.LockInputForAWhile(7f, false);
        }

        //Edits speed
        Player.TurnSpeed /= 1.2f;
        Player.MoveAccell /= 1.3f;
        Player.MoveDecell *= 1.4f;

        //Grants invincibility
        Physics.IgnoreLayerCollision(gameObject.layer, 13, true);
    }
        

    // Update is called once per frame
    void Update()
    {
        CharacterAnimator.SetInteger("Action", 0);
        CharacterAnimator.SetBool("Grounded", true);
        CharacterAnimator.SetFloat("NormalSpeed", -1f);

        counter -= Time.deltaTime;  
        
        //Exit attack
        if (counter <= 0 || !Player.Grounded)
        {
            ExitKick();
        }
    }

    private void ExitKick()
    {
        Actions.SpecialPressed = false;

        KickCollider.SetActive(false);
        Physics.IgnoreLayerCollision(gameObject.layer, 13, false);

        Player.TurnSpeed = originalTurnSpeed;
        Player.MoveAccell = originalAccel;
        Player.MoveDecell = originalDecel;

        CoreCol.gameObject.SetActive(true);
        slideCol.gameObject.SetActive(false);

        Actions.ChangeAction(0);
    }
    private void FixedUpdate()
    {
       
    }

    private void AssignStats()
    {
        KickDuration = Tools.stats.SlideDuration;
        kickDamage = Tools.stats.slideDamage;
        kickForce = Tools.stats.SlideForce;
    }

    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Input = GetComponent<PlayerBinput>();

        CharacterAnimator = Tools.CharacterAnimator;
        Sounds = Tools.SoundControl;
        Effects = Tools.EffectsControl;

        CoreCol = Tools.characterCapsule.GetComponent<CapsuleCollider>();
        slideCol = Tools.crouchCapsule.GetComponent<CapsuleCollider>();


    }
}
