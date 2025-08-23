using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

public class S_Control_EffectsPlayer : S_Player_Base
{


	Camera MainCamera;

	[Header("VFX")]
	[SerializeField] GameObject  _JumpDashParticle;
	[SerializeField] GameObject  _LesserTrails;
	[SerializeField] ParticleSystem RunningDust;
	[SerializeField] ParticleSystem SpeedLinesCharacter;
	[SerializeField] VisualEffect             _SpeedLinesScreen;
	[SerializeField] ParticleSystem SpinDashEnergy;
	[SerializeField] ParticleSystem RailsSparks1;

	[Header("Mouth Sides")]
	Transform _Head;
	Transform LeftMouth, RightMouth;

	[Header("Values")]
	public float RunningDustThreshold;
	public float SpeedLinesThreshold;

	//Trackers
	private bool _canShowLesserTrails;

	private void Start () {
		SpinDashEnergy.Stop();
		RailsSparks1.Stop();
	}

	void Update () {

		HandleMouths();

		if (_PlayerVel._currentRunningSpeed > RunningDustThreshold && _PlayerPhys._isGrounded && RunningDust != null)
		{
			RunningDust.Emit(Random.Range(0, 20));
		}

		HandleSpeedLinesOnCharacter();
		HandleSpeedLinesOnScreen();
		HandleTrailsOnCharacter();
	}

	//VFX
	#region VFX

	//Private

	private void HandleSpeedLinesOnCharacter () {

		if (_PlayerVel._currentRunningSpeed > SpeedLinesThreshold && SpeedLinesCharacter != null && SpeedLinesCharacter.isPlaying == false)
		{
			SpeedLinesCharacter.Play();
		}
		else if ((_PlayerVel._currentRunningSpeed < SpeedLinesThreshold - 5 && SpeedLinesCharacter.isPlaying == true) || Mathf.Abs(_PlayerVel._coreVelocity.y) > _PlayerVel._currentRunningSpeed)
		{
			SpeedLinesCharacter.Stop();
		}

	}

	private void HandleTrailsOnCharacter () {

		if(!_canShowLesserTrails) { return; }
			EnableLesserTrails(_PlayerVel._speedMagnitudeSquared > 40 * 40 && _PlayerVel._horizontalSpeedMagnitude > 10, false);
	}

	//Controls and activates the anime style speedlines on the screen edges based on speed.
	private void HandleSpeedLinesOnScreen () {
		if (_PlayerVel._horizontalSpeedMagnitude > 50)
		{
			//Sets the scale of the effect to fit the camera fov
			float zOffset = (MainCamera.transform.InverseTransformPoint(_SpeedLinesScreen.transform.position)).z;
			Vector2 newScale = S_S_Objects.GetScaleToFitCameraBounds(MainCamera, zOffset, _SpeedLinesScreen.transform, true);

			float intensity = Mathf.Min(_PlayerVel._horizontalSpeedMagnitude / _PlayerPhys._PlayerMovement._currentMaxSpeed , 1.1f);
			intensity = Mathf.Max(Mathf.Abs(intensity - Mathf.Lerp(intensity, 1.1f, 0.5f)) - intensity, intensity - Mathf.Abs(intensity - Mathf.Lerp(intensity, 1.1f, 0.5f)));
			_SpeedLinesScreen.SetFloat("Intensity", intensity);
		}
		else
		{
			_SpeedLinesScreen.SetFloat("Intensity", 0);
		}
	}




	//Public
	public void EnableLesserTrails ( bool enable, bool locked ) {
		if (_LesserTrails.activeSelf != enable)
			_LesserTrails.SetActive(enable);

		_canShowLesserTrails = !locked;
	}

	public void HandleSpinDashEffect ( int amm, float speed, float currentCharge, float maxCharge ) {

		float chargeUsedForEffect = Mathf.Min( currentCharge * 0.15f, 55);


		//Activate spin dash energy effect
		if (!SpinDashEnergy.isPlaying)
		{
			SpinDashEnergy.gameObject.SetActive(true);
			SpinDashEnergy.Play();
		}

		var emission = SpinDashEnergy.emission;
		emission.rateOverTime = chargeUsedForEffect;

		ParticleSystem.MainModule Main = SpinDashEnergy.main;

		//Once fully charged, dim effect slightly.
		if (currentCharge > maxCharge - 0.3f)
			Main.startColor = new Color(0.6f, 0.6f, 0.6f, 1);
		else
			Main.startColor = new Color(1f, 1f, 1f, 1);
	}

	//Disable spin dash energy effect
	public void EndSpinDashEffect () {
		SpinDashEnergy.Stop();
		var emission = SpinDashEnergy.emission;
		emission.rateOverTime = 0f;
		SpinDashEnergy.gameObject.SetActive(false);
	}

	//Spawn an instance of the air dash particle, which is not locked onto the character.
	public void AirDashParticle ( Transform characterReferencePoint ) {
		GameObject JumpDashParticleClone = Instantiate(_JumpDashParticle, characterReferencePoint.position, Quaternion.identity) as GameObject;

		//Affect by player speed.
		if (_PlayerVel._speedMagnitudeSquared > Mathf.Pow(100, 2))
		{
			float scale = _PlayerVel._speedMagnitudeSquared / Mathf.Pow(100,2);
			JumpDashParticleClone.transform.localScale = Vector3.one * scale;
		}

		JumpDashParticleClone.transform.position = characterReferencePoint.position;
		JumpDashParticleClone.transform.rotation = characterReferencePoint.rotation;
	}

	public void HandleGrindSparks ( float speed ) {

		//Activate or deactivate effect
		if (speed > 30 && !RailsSparks1.isPlaying)
			RailsSparks1.Play();

		else if (speed <= 30 && RailsSparks1.isPlaying)
		{
			RailsSparks1.Stop(); return;
		}

		ParticleSystem.MainModule Main = RailsSparks1.main;

		Main.startSpeed = Mathf.Clamp(speed * 0.3f, 20, 60);

	}

	#endregion

	//Not a VFX, but ensures which of the characters two possible mouths is used based on camera angle. Gives the Side Mouth effect.
	private void HandleMouths () {
		Vector3 direction = _CamHandler._HedgeCam.transform.position - _Head.position;
		bool _isFacingRightSide = Vector3.Dot(_Head.forward, direction.normalized) < 0f;

		LeftMouth.localScale = _isFacingRightSide ? Vector3.zero : Vector3.one;
		RightMouth.localScale = !_isFacingRightSide ? Vector3.zero : Vector3.one;
	}

	public override void AssignTools () {
		base.AssignTools();

		_Head = _Tools.Head;
		LeftMouth = _Tools.LeftMouth;
		RightMouth = _Tools.RightMouth;

		MainCamera = Camera.main;
	}

}
