using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTools : MonoBehaviour
{
    [Header("Stats")]
    public SurfaceStatsCharacter stats;
    public CoreStatsCharacter coreStats;
    public CameraStats camStats;

    [Header("Key Objects")]
    public GameObject JumpBall;
    public GameObject characterCapsule;
    public GameObject crouchCapsule;
    public GameObject pullItems;
    public GameObject SpinDashBall;


    public GameObject shield;
    public GameObject homingIcons;
    public GameObject normalIcon;
    public GameObject weakIcon;
    public GameObject dropShadow;

    public Transform MainCamera;
    public Transform cameraTarget;
    public Transform constantTarget;

    public GameObject movingRing;

    [Header("Model / Skin")]
    public Animator CharacterAnimator;
    public SonicSoundsControl SoundControl;
    public SonicEffectsControl EffectsControl;
    public Animator BallAnimator;
    public SkinnedMeshRenderer[] PlayerSkin;
    public GameObject DropSpinBall;
    public Transform mainSkin;
    public Transform PlayerSkinTransform;
    public Transform DirectionReference;
    public VolumeTrailRenderer HomingTrailScript;
    public GameObject HomingTrailContainer;
    public GameObject HomingTrail;

    public Transform HandGripPoint;
    public Transform FeetPoint;

    [Header("Particles")]
    public Image FadeOutImage;

    public ParticleSystem DropEffect;
    public GameObject JumpDashParticle;
}
