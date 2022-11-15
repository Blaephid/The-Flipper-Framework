using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Surface Stats")]
public class SurfaceStatsCharacter : ScriptableObject
{
    public string Title;

    [Header("Core movement")]

    [Header("Movement Values")]

    public float StartAccell = 0.16f;



    public float TangentialDrag = 7.5f;

    public float TurnSpeed = 70f;
    [HideInInspector]public float SlowedTurnSpeed = 200f;


    public float StartTopSpeed = 90f;
    public float StartMaxSpeed = 170f;
    public float StartMaxFallingSpeed = -400f;
    public float StartJumpPower = 1.2f;

    
    public float MoveDecell = 1.05f;

    public float AirDecell = 1.25f;




    [Header("Slope effects")]


    [Tooltip("This is multiplied with the force of a slope to determine the additional force.")]
    public float generalHillMultiplier = 1;
 



    [Header("AirMovementExtras")]
    public float AirControlAmmount = 0.8f;
    public float AirSkiddingForce = 6;
    public Vector3 fallGravity = new Vector3(0f, -1.5f, 0f);



    [Header("Skid & Stop")]
    public float SpeedToStopAt = 16;

    public float SkiddingStartPoint = 5;
    public float SkiddingIntensity = -4;

    [Header("Jump")]
    public float StartJumpDuration = 0.2f;
    public float StartSlopedJumpDuration = 0.2f;
    public float StartJumpSpeed = 4;

    [Header("Adittional Jumps")]
    public bool canDoubleJump = true;
    public bool canTripleJump = false;

    public float doubleJumpSpeed = 4.5f;
    public float doubleJumpDuration = 0.14f;

    [Header("QuickStep")]
    public float StepSpeed = 50f;
    public float StepDistance = 8f;
    public float AirStepSpeed = 45f;
    public float AirStepDistance = 7f;

 
    [Header ("Homing")]

    public float HomingAttackSpeed = 70;
    public float HomingTimerLimit = 1;
    public float HomingSuccessDelay = 0.4f;
    //public float FacingAmount;


    [Header ("Air Dash")]

    public float AirDashSpeed = 60;
    public float AirDashDuration = 0.4f;

    [Header("Spin Charge")]
    public float SpinDashChargingSpeed = 1.08f;
    public float MinimunCharge = 20;
    public float MaximunCharge = 100;
    public float SpinDashStillForce = 1.05f;


    [Header("Bounce")]
    public float DropSpeed = 100;
    public float BounceMaxSpeed = 140;
    public List<float> BounceUpSpeeds;
    public float BounceHaltFactor = 0.95f;
    public float BounceCoolDown = 0.18f;



    [Header("Ring Road")]
    //[SerializeField] float DashingTimerLimit;
    public float DashSpeed = 100;
    public float EndingSpeedFactor = 1.2f;
    public float MinimumEndingSpeed = 60;


    [Header("Drop Dash")]
    public float DropDashChargingSpeed = 1.2f;
    public float DropMinimunCharge = 40;
    public float DropMaximunCharge = 150;



    [Header("Hurt")]


    public int InvincibilityTime = 90;
    public int MaxRingLoss = 20;


    [Header("Wall Effects")]
    public float scrapeModi = 1f;
    public float climbModi = 1f;




}
