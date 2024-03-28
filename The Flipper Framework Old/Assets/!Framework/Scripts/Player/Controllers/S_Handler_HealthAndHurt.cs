using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

[RequireComponent(typeof(S_Action04_Hurt))]
public class S_Handler_HealthAndHurt : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Manager_LevelProgress _LevelHandler;
	private S_Interaction_Objects _Objects;
	private S_Handler_Camera      _CamHandler;
	private S_Control_SoundsPlayer _Sounds;
	private S_Handler_CharacterAttacks      _Attacks;
	private S_Action04_Hurt                 _HurtAction;

	private GameObject            _JumpBall;
	private Animator              _CharacterAnimator;
	private SkinnedMeshRenderer[] _SonicSkins;
	private Transform             _MainSkin;
	private CapsuleCollider _CharacterCapsule;

	[HideInInspector]
	public Image        _FadeOutImage;

	private GameObject  _MovingRing;
	private GameObject  _ReleaseDirection;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private S_Enums.HurtResponse _whatResponse_;
	private int         _maxRingLoss_;
	private float       _ringReleaseSpeed_;
	private float       _ringArcSpeed_;
	private int         _invincibilityTime_;
	private Vector2       _flickerTime_;
	private float       _damageShakeAmmount_;
	private float       _enemyHitShakeAmmount_;
	private AnimationCurve _RingsLostInSpawnByAmount_;
	private Vector3        _respawnAfter_;

	private LayerMask   _BonkWall_;
	#endregion
	// Trackers
	#region trackers

	//Health
	[HideInInspector]
	public float _ringAmount; //The amount of health the player has. Goes down on hit, up on gaining rings.

	//States
	public bool         _isHurt;
	public bool         _isInvincible; //Cannot take damage while this is true. Temporarily set on hit.
	public bool         _isDead; //If the player has been killed
	public bool         _hasShield = false; //Shields can be gained from monitors and grant one free hit
	public bool         _inHurtStateBeforeDamage; //Frontiers hurt responses mean not takind damage until landing after being hit.
	public bool         _wasHurtWithoutKnockback; //Frontiers hurt responses mean not takind damage until landing after being hit.

	//Counters
	private int         _counter; //How long is hurt for, when to end invincibility.
	private float       _flickerCounter; //When invncible after taking damage, skin will flicker, this handles when to show and hide it.
	private int         _deadCounter = 0; //Follows how long the player has been dead to allow respawning and resetting.

	//Release rings on hurt
	private bool        _isReleasingRings = false;
	private int         _ringsToLose;       //Tracks how many rings to be shot out, doesn't decrease 1 per ring spawned, but does decrease.
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyScript();
	}

	private void FixedUpdate () {
		HandleDamaged();

		//For debugging purposes, kill the player at any time by pressing a button.
		if (_Input.killBindPressed)
		{
			Die();
		}

		CheckBonk();
	}

	//Any collisions involved in losing health are here, rather than in the object interaction script.
	public void OnTriggerEnter ( Collider other ) {

		switch (other.tag)
		{
			case "Hazard":
				DamagePlayer();
				_CamHandler._HedgeCam.ApplyCameraShake(_damageShakeAmmount_, 60);
				return;
			case "Enemy":
				_CamHandler._HedgeCam.ApplyCameraShake(_enemyHitShakeAmmount_, 30);

				if (!_Attacks.AttemptAttackOnContact(other, S_Enums.AttackTargets.Enemy))
				{
					DamagePlayer();
				}
				return;
			case "Pit":
				_Sounds.DieSound();
				Die();
				return;
		}
	}

	public void OnCollisionEnter ( Collision collision ) {
		
		switch (collision.collider.tag)
		{
			case "Hazard":
				DamagePlayer();
				_CamHandler._HedgeCam.ApplyCameraShake(_damageShakeAmmount_, 60);
				return;
		}
	}

	//Called every frame when overlapping a trigger
	public void OnTriggerStay ( Collider col ) {
		//Pits force the camera to look down from above
		if (col.tag == "Pit")
		{
			_CamHandler._HedgeCam.SetCameraHeightOnly(85, 2, 1);
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Called every frame after being damaged. Handles invincibility times and other responses.
	private void HandleDamaged () {

		//Check limit of invicibility and ensure it is applied and shown
		_counter += 1;
		if (_counter < _invincibilityTime_)
		{
			_isInvincible = true;
			SkinFlicker();
		}
		else if(_isInvincible)
		{
			_isInvincible = false;
			_Actions._ActionDefault.HideCurrentSkins(true);
		}

		//When rings are lost, this is enabled to unleash rings from character each frame.
		if (_isReleasingRings)
		{
			DroppingRings();
		}
	}

	//Handles visibility to represent invincibility
	private void SkinFlicker () {
		_flickerCounter++; //Increases counter

		//Skins aren't visible for flicker time y
		if (_flickerCounter < 0)
		{
			_Actions._ActionDefault.HideCurrentSkins(true);
		}
		//Skins are visible for flicker time x
		else
		{
			_Actions._ActionDefault.HideCurrentSkins(false);
		}
		//When enough time spent visible, set to time invisible
		if (_flickerCounter > _flickerTime_.x)
		{
			_flickerCounter = -_flickerTime_.y;
		}
	}

	//Sends out the lost rings as gameObjects in the scene, to be picked up again. 
	private void DroppingRings () {

		//As long as there are rings to lose, then one will be spawned.
		if (_ringsToLose > 0)
		{
			//Get spawn location
			Vector3 pos = transform.position;
			pos.y += 1;

			//Spawn a ring based on the moving ring prefab and shoot it out away from the player
			GameObject movingRing = Instantiate(_MovingRing, pos, Quaternion.identity);
			movingRing.transform.parent = null;
			movingRing.GetComponent<Rigidbody>().velocity = Vector3.zero;

			Vector3 launchDirection = _ReleaseDirection.transform.forward * _ringReleaseSpeed_;
			launchDirection += new Vector3 (_HurtAction._knockbackDirection.x, 0, _HurtAction._knockbackDirection.z) * 850;// Apply additional force towards where player is beng sent
			movingRing.GetComponent<Rigidbody>().AddForce(launchDirection, ForceMode.Acceleration); //Apply force out from player

			_ReleaseDirection.transform.Rotate(0, _ringArcSpeed_, 0); //Change the direction to fire ring in next spawn.

			_ringsToLose -= Mathf.Max((int)_RingsLostInSpawnByAmount_.Evaluate(_ringsToLose), 1); //The number of rings to lose spent depends on how many there are. If it was always one cost per spawn, it would get distracting and heavy at high damage.
		}
		//When out of rings to spawn, end the method.
		else
		{
			_isReleasingRings = false;
		}
	}

	//Called when the player has to die, activating trackers for the death state, and disables control, while still entering the hurt state.
	private void Die () {
		//If not dead already.
		if (!_isDead)
		{
			//Effects
			_Sounds.DieSound();
			_JumpBall.SetActive(false);
			_CharacterAnimator.SetBool("Dead", true);

			//Trackers
			_isDead = true;

			//Set public
			_ringAmount = 0;
			_PlayerPhys._listOfCanControl.Add(false);

			//Enter the hurt action until respawn
			StartCoroutine(TrackDeath());
			if (!_HurtAction.enabled) _HurtAction.StartAction();
		}
	}

	//A coroutine that updates independantly even when the player object is disabled. This handles the different events that happen when dead until respawning.
	private IEnumerator TrackDeath () {

		while (_isDead)
		{
			yield return new WaitForFixedUpdate();

			_Input._move = Vector3.zero;
			_deadCounter += 1;

			//Start fading out the screen
			if (_deadCounter > _respawnAfter_.x && _deadCounter < _respawnAfter_.y)
			{
				//Gets the percentage value of the movement from start fade out time to end fade out time
				float lerpTotal = _respawnAfter_.y - _respawnAfter_.x; //The difference between start and end
				float lerpAmount = (_deadCounter - _respawnAfter_.x); //The amount after the start
				lerpAmount = lerpAmount / lerpTotal;    //The progress as a percentage

				//Change colour according to the larp value
				Color imageColour = Color.black;
				imageColour.a = 1;
				_FadeOutImage.color = Color.Lerp(_FadeOutImage.color, imageColour, lerpAmount);
			}
			//When the screen is fully faded, respawn elements
			else if (_deadCounter == _respawnAfter_.y)
			{
				_CharacterCapsule.gameObject.SetActive(false);  //Disables the player now that they can't be seen, this will prevent other updates outside of this coroutine.
				_LevelHandler.RespawnObjects();

				_counter = _invincibilityTime_; //Ends the counter for flickering so the character will be fully visible on respawn.
			}
			//And after being dead for long enough, respawn the player.
			else if (_deadCounter == _respawnAfter_.z)
			{
				ResetStatsOnRespawn();

				//Move player back to checkPoint
				_LevelHandler.ResetToCheckPoint();

				_CharacterCapsule.gameObject.SetActive(true); ; //Reenables character object to allow all other updates to happen again, and retrigger any collisions at the new location.
			}
			//Removed screen overlay to reveal new location
			else if (_deadCounter > _respawnAfter_.z)
			{
				float lerpAmount = (_deadCounter - _respawnAfter_.x); //The amount after the start
				lerpAmount = lerpAmount / 10;    //The progress as a percentage

				//Change colour according to the lerp value
				Color imageColour = Color.black;
				imageColour.a = 0;
				_FadeOutImage.color = Color.Lerp(_FadeOutImage.color, imageColour, lerpAmount);

				//End state
				if (_deadCounter == _respawnAfter_.z + 10)
				{
					_isDead = false;
					_deadCounter = 0;
				}
			}
		}
	}

	private void ResetStatsOnRespawn() {
		//Visual
		_CharacterAnimator.SetBool("Dead", false);

		//Reset trackers
		_PlayerPhys._listOfCanControl.Clear();
		_PlayerPhys._listOfCanTurns.Clear();
		_PlayerPhys._canBeGrounded = true;
		_PlayerPhys._arePhysicsOn = true;

		_PlayerPhys.SetTotalVelocity(Vector3.zero, new Vector2(0, 0));
	}

	//Bonking refers to rebounding off solid surfaces when moving into them at high speed.
	//Depending on the current state, will check if should bonk against walls based on speed
	private void CheckBonk () {
		switch (_Actions._whatAction)
		{
			case S_Enums.PrimaryPlayerStates.Default:
				if (_PlayerPhys._horizontalSpeedMagnitude > 50) TryBonk();
				break;
			case S_Enums.PrimaryPlayerStates.Jump:
				if (_PlayerPhys._horizontalSpeedMagnitude > 40) TryBonk();
				break;
			case S_Enums.PrimaryPlayerStates.JumpDash:
				if (_PlayerPhys._horizontalSpeedMagnitude > 30) TryBonk();
				break;
		}
	}

	//Checks walls infront of the character, ready to rebound off if too close.
	private void TryBonk () {

		Vector3 movingDirection = _PlayerPhys._RB.velocity.normalized; //Project on plane makes direction relevant to transform so it will only check in front of player, not below or above
		float distance = Mathf.Max(_PlayerPhys._horizontalSpeedMagnitude * Time.deltaTime * 2.5f, 2); //Uses timme.delta time to check where the character should probably be next frame.
		Vector3 sphereStartOffset = transform.up * (_CharacterCapsule.height / 2); // Since capsule casts take two spheres placed and moved along a direction, this is for the placement of those spheres.

		//Checks for a wall, and if the direction of it is similar to movement direction, ready bonk.
		if (Physics.CapsuleCast(transform.position + sphereStartOffset, transform.position,
			_CharacterCapsule.radius / 1.5f, movingDirection, out RaycastHit wallHit, distance, _BonkWall_))
		{
			float directionAngle = Vector3.Angle(movingDirection, wallHit.point - transform.position); //Difference between moving direction and direction of collision
			float intoAngle = Vector3.Angle(movingDirection, wallHit.normal); //Difference between the player movement direction and wall they're going into. 180 means running straight into a wall facing directily flat on.
			float surfaceAngle = Vector3.Angle(transform.up, wallHit.normal); //Difference between character upwards direction and surface upwards direction
			if (directionAngle < 35 && surfaceAngle > 50 && intoAngle > 158)
				StartCoroutine(DelayBonk());
		}
	}

	//Since wall climbing and running are based on running into walls, this gives those a chance before bonking.
	IEnumerator DelayBonk () {
		Vector3 rememberDirection = _MainSkin.forward; //Saves the direction so the player can't rotate from it until bonk is over
		float rememberSpeed = _PlayerPhys._horizontalSpeedMagnitude;

		//If already in a wallrunning state, then this can't transition into a wall climb, so rebound off immediately.
		if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.WallRunning)
		{
			_HurtAction._knockbackDirection = -_PlayerPhys._previousVelocities[1].normalized;
			_HurtAction._wasHit = false;
			_Actions._ActionHurt.StartAction();
		}
		else
		{
			//Trigger the 3 frame delay, ensuring player can't move or rotate until it is over.
			for (int i = 0 ; i < 3 ; i++)
			{
				yield return new WaitForFixedUpdate();

				if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.Hurt) { break; }

					_PlayerPhys.SetTotalVelocity(Vector3.zero, new Vector2(1, 0));
				_PlayerPhys._horizontalSpeedMagnitude = rememberSpeed; //Wont affect velocity, but this will trick trackers using speed into thinking the character is still moving.
				_MainSkin.forward = rememberDirection;
			}

			//If still not in a wallrunning state or been hurt, then rebound off the wall.
			if (_Actions._whatAction != S_Enums.PrimaryPlayerStates.WallRunning && _Actions._whatAction != S_Enums.PrimaryPlayerStates.Hurt)
			{
				_HurtAction._knockbackDirection = -_PlayerPhys._previousVelocities[3].normalized;
				_HurtAction._wasHit = false;
				_Actions._ActionHurt.StartAction();
			}
		}
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Whenever the player has been hit, will trigger the action and deal damage to the health or shield.
	public void DamagePlayer () {

		if(_isInvincible) { return; }

		if (!_Actions._ActionHurt.enabled)
		{
			//The different responses determine when the player takes damage, either immdieately, or upon landing in the hurt state.
			switch (_whatResponse_)
			{
				//Damaged immediately but won't be knocked back or have velocity greatly changed.
				case S_Enums.HurtResponse.Normal:
					CheckHealth();
					_HurtAction._knockbackDirection = Vector3.zero;
					_HurtAction._wasHit = true;
					_HurtAction.StartAction();
					break;
				//Damaged immediately and will enter a seperate knockback state.
				case S_Enums.HurtResponse.ResetSpeed:
					CheckHealth();
					_HurtAction._knockbackDirection = -_MainSkin.forward;
					_HurtAction._wasHit = true;
					_HurtAction.StartAction();
					break;
				//Won't be damaged until the EventOnGrounded action in the hurt script. This will then call the CheckHealth script
				case S_Enums.HurtResponse.Frontiers:
					_inHurtStateBeforeDamage = true;
					_HurtAction._knockbackDirection = -_PlayerPhys._previousVelocities[1].normalized;
					_HurtAction._wasHit = true;
					_HurtAction.StartAction();
					break;
				//Same as frontiers response, but if should die, will do so immediately.
				case S_Enums.HurtResponse.FrontiersWithoutDeathDelay:
					_HurtAction._knockbackDirection = -_PlayerPhys._previousVelocities[1].normalized;
					_HurtAction._wasHit = true;
					if (_ringAmount > 0 || _hasShield)
					{
						_inHurtStateBeforeDamage = true;
						_HurtAction.StartAction();
					}
					else
					{
						_HurtAction._knockbackDirection.y = -0.5f;
						Die();
					}
					break;
			}


		}
	}

	//Called either upon hit or upon handling hit (depending on response enum), checks what health should be lost.
	public void CheckHealth () {
		//Remove shield rather than take damage
		if (_hasShield)
		{
			//Lose Shield
			_Sounds.SpikedSound();
			SetShield(false);
		}
		//Otherwise, either lose rings or die
		else if (_ringAmount > 0)
		{
			LoseRings();
		}
		else if (_ringAmount <= 0)
		{
			Die();
		}

	}

	//Sets values when hurt, and readies rings to be fired out.
	public void LoseRings ( float damage = 0 ) {
		//Gets how many rings to lose. This will typically me the max ring loss, but if that's set to zero, it will be all rings instead.
		damage = damage <= 0 ? _maxRingLoss_ : damage;
		damage = damage <= 0 ? _ringAmount : damage;
		damage = Mathf.Clamp(damage, 0, _ringAmount);

		_ringAmount = (int) _ringAmount - damage; //Ensures it will be decreased to a whole number, not a decimal.

		//Set time to be in hurt state
		_counter = 0;

		//Effects
		_Sounds.RingLossSound();

		//Readies the rings being spawned as objects in the world
		if (!_isReleasingRings)
		{
			_ringsToLose = (int)damage;
			_isReleasingRings = true;
		}
	}


	//Called when a ring is picked up. Doesn't apply it until the end of the frame to ensure that only one ring is gained per frame, ignoring potential multiple collisions.
	public IEnumerator GainRing (float amount, Collider col, GameObject Particle) {
		Instantiate(Particle, col.transform.position, Quaternion.identity);
		Destroy(col.gameObject);

		float ThisFramesRingCount = _ringAmount;
		yield return new WaitForEndOfFrame();

		_ringAmount = Mathf.Clamp(ThisFramesRingCount + amount, _ringAmount, ThisFramesRingCount + 100); 
	}

	//Called whenever the player should gain or lose a shield, which blocks one hit.
	public void SetShield ( bool isOn ) {
		_hasShield = isOn;
		_Objects.ShieldObject.SetActive(isOn);
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//If not assigned already, sets the tools and stats and gets placement in Action Manager's action list.
	public void ReadyScript () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			_counter = _invincibilityTime_;
			_ReleaseDirection = new GameObject();
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_LevelHandler = GetComponent<S_Manager_LevelProgress>();
		_Actions = GetComponent<S_ActionManager>();
		_Objects = GetComponent<S_Interaction_Objects>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();
		_Attacks = GetComponent<S_Handler_CharacterAttacks>();
		_HurtAction = GetComponent<S_Action04_Hurt>();

		_CharacterCapsule = _Tools.characterCapsule.GetComponent<CapsuleCollider>();
		_MainSkin = _Tools.mainSkin;
		_JumpBall = _Tools.JumpBall;
		_Sounds = _Tools.SoundControl;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_SonicSkins = _Tools.PlayerSkin;
		_MovingRing = _Tools.movingRing;
		_FadeOutImage = _Tools.FadeOutImage;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_invincibilityTime_ = _Tools.Stats.WhenHurt.invincibilityTime;
		_flickerTime_ = _Tools.Stats.WhenHurt.flickerTimes;

		_maxRingLoss_ = _Tools.Stats.WhenHurt.maxRingLoss;
		_ringReleaseSpeed_ = _Tools.Stats.WhenHurt.ringReleaseSpeed;
		_ringArcSpeed_ = _Tools.Stats.WhenHurt.ringArcSpeed;
		_RingsLostInSpawnByAmount_ = _Tools.Stats.WhenHurt.RingsLostInSpawnByAmount;

		_damageShakeAmmount_ = _Tools.Stats.EnemyInteraction.damageShakeAmmount;
		_enemyHitShakeAmmount_ = _Tools.Stats.EnemyInteraction.hitShakeAmmount;

		_respawnAfter_ = _Tools.Stats.WhenHurt.respawnAfter;

		_BonkWall_ = _Tools.Stats.WhenBonked.BonkOnWalls;

		_whatResponse_ = _Tools.Stats.KnockbackStats.whatResponse;
	}
	#endregion
}
