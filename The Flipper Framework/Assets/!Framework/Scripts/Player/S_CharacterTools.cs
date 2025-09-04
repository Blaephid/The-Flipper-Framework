using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class S_CharacterTools : MonoBehaviour
{

	[Header("Stats")]
	public S_O_CharacterStats       Stats;
	public S_O_CharacterLevelUpStats       LevelUpStats;
	public S_O_CameraStats  CameraStats;

	[Header("Attach from Action Manager")]
	public S_ActionManager  _ActionManager;
	public S_Interaction_Pathers  PathInteraction;
	public S_Handler_Camera       CamHandler;
	public S_PlayerEvents   PlayerEvents;

	[Header("Components")]
	public S_Handler_HealthAndHurt _HealthHandler;
	public S_PlayerPhysics _PlayerPhys;
	public S_PlayerVelocity _PlayerVel;
	public S_PlayerMovement _PlayerMove;
	public S_PlayerCoreValues _CoreValues;

	[Header("Key Objects")]
	public GameObject               Root;

	[Header("UI")]
	public S_Spawn_UI             UISpawner;

	[Header ("Colliders")]
	public GameObject               CharacterCapsule;
	public GameObject             StandingCapsule;
	public GameObject               CrouchCapsule;
	public GameObject               DisabledCapsule;

	[Header("Model / Skin")]
	public SkinnedMeshRenderer      SkinRenderer;
	public Transform                MainSkin;
	public Transform                CharacterModelOffset;
	public SkinnedMeshRenderer      CurledBall;

	[Header("Mouth Sides")]
	public Transform              Head;
	public Transform                LeftMouth, RightMouth;

	[Header("Balls")]
	public GameObject       JumpBall;

	[Header("External Objects")]
	public GameObject       Shield;
	public GameObject       DropShadow;

	[Header("Camera Related")]
	public Transform                CameraTarget;
	public Transform                ConstantTarget;
	public CinemachineBrain       MainCamera;


	[Header("Prefabs")]
	public GameObject       MovingRingObject;

	[Header("Location References")]
	public Transform        HandGripPoint;
	public Transform        FeetPoint;
	public Transform        CenterOfMass;

	[Header("Control")]
	public Animator                 BallAnimator;
	public Animator                 CharacterAnimator;
	public S_Control_SoundsPlayer           SoundControl;
	public S_Control_EffectsPlayer  EffectsControl;

	[Header("Effects")]
	public GameObject               HomingTrailContainer;
	public GameObject               HomingTrail;
	public GameObject             BoostCone;
	public ParticleSystem   DropEffect;

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

			_ActionManager._ObjectForInteractions.GetComponent<S_Manager_LevelProgress>()._Spawner = Spawner;
			//if (Spawner._launch) { _ActionManager._ObjectForInteractions.GetComponent<S_Manager_LevelProgress>()._respawnLaunch = Spawner._launchOnSpawnData_; }

		}
	}
}
