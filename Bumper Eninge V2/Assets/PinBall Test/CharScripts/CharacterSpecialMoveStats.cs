//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(fileName = "New Special", menuName = "SonicGT/Character/Specials")]
//public class CharacterSpecialMoveStats : ScriptableObject {

//    public SpecialAction selectedAction = SpecialAction.Sonic_DropDash;

//    [Header("Passive")]

//    public bool RollingShield;
//    public bool ShieldSpecialMoves;
//    public float BoostAndShieldDamageRadious = 13;
//    public bool MomentumHomingAttack = false;
//    public bool Super = false;


//    [Header("DropDash")]
//    public float SpinDashChargedEffectAmm = 0;
//    public float SpinDashChargingSpeed = 1.3f;
//    public float MinimunCharge = 80;
//    public float MaximunCharge = 210;
//    public float ReleaseShakeAmmount = 0;
//    public float DSForceIntoRollfor = 0.6f;

//    [Header("BounceDash")]
//    public float BDHaltFactor = 0.75f;
//    public float BDStartCharge = 130, 
//        BDDropSpeed = 200, 
//        BounceDashSpeedMultiplier = 0.72f, 
//        BounceMaximunCharge = 240,
//        BDForceIntoRollfor = 0.6f;


//    //Teleport
//    [Header("Teleport")]
//    public AnimationCurve teleportAcel;
//    public AnimationCurve teleportTimeDistortion;
//    public float teleportAttackSpeed = 800, 
//        teleportExitHangTime = 0.1f, 
//        teleporAnimRot = 20, 
//        teleportTurnRadious = 150,
//        teleportDashDistance = 50;


//    [Header("Ray Flight")]

//    public float FlightdirChangeSpeed = 6;
//    public float MinFlightSpeed = 60, flightControl = 0.025f; //Minimal Speed and control
//    public Vector2 FlightAngles = new Vector2(-0.6f, 0.3f); // Angle of Lift and Drop
//    public float FlightUpwardsDrag = 0.995f, FlightDownwardsMultiplier = 1.0075f; // Speed Mutipliers
//    public Vector2 LiftSpeeds = new Vector2(70,90);
//    public AnimationCurve flightAcelerationOverSpeed;


//    [Header("Boost")]

//    public float NewAceleration = 1.3f;
//    public float RingRemoveFrames = 3;    
//    public float RailBoost = 30;

//}
