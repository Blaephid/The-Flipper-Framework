using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_CharacterTools : MonoBehaviour
{
    [Header("Stats")]
    public SurfaceStatsCharacter stats;
    public CoreStatsCharacter coreStats;
    public CameraStats camStats;

    [Header("Key Objects")]
    [Header ("Colliders")]
    public GameObject characterCapsule;
    public GameObject crouchCapsule;
    public GameObject faceHit;
    public GameObject angleSphere;

    [Header("Balls")]
    public GameObject JumpBall;       
    public GameObject SpinDashBall;


    public GameObject shield;

    [Header("Homing")]
    public GameObject homingIcons;
    public GameObject normalIcon;
    public GameObject weakIcon;
    public GameObject dropShadow;

    [Header("Camera")]
    public Transform MainCamera;
    public Transform cameraTarget;
    public Transform constantTarget;

    [Header("Prefabs")]
    public GameObject movingRing;

    [Header("Model / Skin")]
          
    public SkinnedMeshRenderer[] PlayerSkin;
    public GameObject DropSpinBall;
    public Transform mainSkin;
    public Transform PlayerSkinTransform;
    

    [Header("Location References")]

    public Transform HandGripPoint;
    public Transform FeetPoint;
    public Transform DirectionReference;
    public Transform CollisionPoint;

    [Header("Control")]
    public Animator BallAnimator;
    public Animator CharacterAnimator;
    public S_Control_SoundsPlayer SoundControl;
    public S_Control_EffectsPlayer EffectsControl;

    [Header("Particles")]
    public Image FadeOutImage;

    public ParticleSystem DropEffect;
    public GameObject JumpDashParticle;

    [Header("Effects")]
    public S_VolumeTrailRenderer HomingTrailScript;
    public GameObject HomingTrailContainer;
    public GameObject HomingTrail;
}
