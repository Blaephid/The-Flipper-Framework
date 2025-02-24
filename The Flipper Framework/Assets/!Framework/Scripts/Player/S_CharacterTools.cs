using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class S_CharacterTools : MonoBehaviour
{

	[Header("Stats")]
	public S_O_CharacterStats	Stats;
	public S_O_CameraStats	CameraStats;

	[Header("Attach from Action Manager")]
	public S_ActionManager	_ActionManager;
	public S_Interaction_Pathers  PathInteraction;
	public S_Handler_Camera       CamHandler;
	public S_PlayerEvents	PlayerEvents;

	[Header("Key Objects")]
	[Header("UI")]
	public S_Spawn_UI             UISpawner;

	[Header ("Colliders")]
	public GameObject		CharacterCapsule;
	public GameObject             StandingCapsule;
	public GameObject		CrouchCapsule;

	[Header("Model / Skin")]
	public SkinnedMeshRenderer	SkinRenderer;
	public Transform		MainSkin;
	public Transform		CharacterModelOffset;

	[Header("Mouth Sides")]
	public Transform              Head;
	public Transform		LeftMouth, RightMouth;

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
	public Transform		CameraTarget;
	public Transform		ConstantTarget;
	public CinemachineBrain       MainCamera;


	[Header("Prefabs")]
	public GameObject	MovingRingObject;

	[Header("Location References")]
	public Transform	HandGripPoint;
	public Transform	FeetPoint;

	[Header("Control")]
	public Animator			BallAnimator;
	public Animator			CharacterAnimator;
	public S_Control_SoundsPlayer		SoundControl;
	public S_Control_EffectsPlayer	EffectsControl;

	[Header("Particles")]
	public ParticleSystem	DropEffect;
	public GameObject		JumpDashParticle;

	[Header("Effects")]
	public S_VolumeTrailRenderer		HomingTrailScript;
	public GameObject		HomingTrailContainer;
	public GameObject		HomingTrail;
	public GameObject             BoostCone;

	//Sets missing or hidden tools when spawned by SpawnCharacter
	private void Awake () {
		S_SpawnCharacter Spawner = GetComponentInParent<S_SpawnCharacter>();

		if (Spawner != null)
		{

			//Setting values from spawner
			MainCamera = Spawner._CameraBrain;
			S_SpawnCharacter._SpawnedPlayer = transform;

			//Rotation
			transform.up = Spawner.transform.up;
			MainSkin.rotation = Quaternion.LookRotation(Spawner.transform.forward, transform.up);

			StartCoroutine(DelayForValuesToBeSetBeforeLaunch(Spawner));
		}
	}

	IEnumerator DelayForValuesToBeSetBeforeLaunch (S_SpawnCharacter Spawner) {

		for (int i = 0 ; i < 2 ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		//Applying launch
		if (_ActionManager._ObjectForInteractions.TryGetComponent(out S_Interaction_Objects Objects))
		{
			S_Structs.LaunchPlayerData launch = Spawner._launchOnSpawnData_;
			if(launch._force_ <= 0 && launch._directionToUse_.sqrMagnitude <= 1) { yield break; }

			Objects.ApplyLaunchEffects(launch);
			Objects.LaunchInDirection(launch._directionToUse_, launch._force_, Vector3.zero, Spawner.transform, true);
		}
	}
}
