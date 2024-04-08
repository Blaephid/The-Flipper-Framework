using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_CharacterTools : MonoBehaviour
{
	[Header("Stats")]
	public S_O_CharacterStats	Stats;
	public S_O_CameraStats	CameraStats;

	[Header("Key Objects")]
	[Header ("Colliders")]
	public GameObject		CharacterCapsule;
	public GameObject		CrouchCapsule;

	[Header("Model / Skin")]
	public SkinnedMeshRenderer[]	PlayerSkins;
	public Transform		MainSkin;
	public Transform		PlayerSkinTransform;

	[Header("Balls")]
	public GameObject	JumpBall;
	public GameObject	SpinDashBall;

	[Header("External Objects")]
	public GameObject	Shield;
	public GameObject	DropShadow;

	[Header("Homing")]
	public GameObject	homingIcons;
	public GameObject	NormalIcon;
	public GameObject	DamageIcon;

	[Header("Camera Related")]
	public Transform		MainCamera;
	public S_Handler_Camera	CamHandler;
	public Transform		CameraTarget;
	public Transform		ConstantTarget;

	[Header("Interactions")]
	public S_Interaction_Pathers	PathInteraction;

	[Header("Prefabs")]
	public GameObject	MovingRingObject;
	public GameObject	BoostUI;

	[Header("Location References")]
	public Transform	HandGripPoint;
	public Transform	FeetPoint;

	[Header("Control")]
	public Animator			BallAnimator;
	public Animator			CharacterAnimator;
	public S_Control_SoundsPlayer		SoundControl;
	public S_Control_EffectsPlayer	EffectsControl;

	[Header("Particles")]
	public Image		FadeOutImage;
	public ParticleSystem	DropEffect;
	public GameObject		JumpDashParticle;

	[Header("Effects")]
	public S_VolumeTrailRenderer		HomingTrailScript;
	public GameObject		HomingTrailContainer;
	public GameObject		HomingTrail;
	public GameObject             BoostCone;
}
