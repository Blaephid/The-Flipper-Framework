using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFX", menuName = "SonicGT/Character/New SFX")]
public class CharacterSFX : ScriptableObject
{
    [Header("Ground Interaction")]
    public AudioClip[] FootStepsGrass;
    [Space]
    public AudioClip[] FootStepsWood;
    [Space]
    public AudioClip[] FootStepsStone;
    [Space]
    public AudioClip[] FootStepsDirt;
    [Space]
    public AudioClip[] FootStepsMetal;
    
    public AudioClip[] FootStepsGlass;
    [Space]
    public AudioClip[] FootStepsWater;
    [Space]
    public AudioClip[] LandingSounds;
    [Space]
    public AudioClip[] BrakeSounds;
    [Space]
    public AudioClip[] RollingSound;

    [Header("Actions")]
    public AudioClip Jumping;
    public AudioClip HomingAttack;
    public AudioClip Spin;
    public AudioClip SpinDash;
    public AudioClip SpinDashRelease;
    public AudioClip BounceStart;
    public AudioClip BounceImpact;
    public AudioClip StompImpact;
    public AudioClip RingLoss;
    public AudioClip Die;
    public AudioClip Spiked;
    public AudioClip Boost;
    public AudioClip ExtraLife;

    [Header ("Rails and Ziplines")]
    public AudioClip EnterRail;
    public AudioClip RailLoop;
    public AudioClip EnterZip;
    public AudioClip ZipLineLoop;

    [Header("WindSounds")]
    
    public float runningMinSpeedSound = 60;
    public float fallingMinSpeedSound = 120;    
    public AnimationCurve pitchRate;
    public AnimationCurve VolumeRate;
    public float windStopLerp = 2;
    public AudioClip RaisingSound, FallingSound;



}
