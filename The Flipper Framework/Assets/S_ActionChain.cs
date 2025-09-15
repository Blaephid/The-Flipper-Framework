using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;

public class S_ActionChain : S_Player_Base
{

	S_Control_EffectsPlayer _Effects;

	public bool _isActionChainEnabled = true;


	[NonSerialized] public List<string> _ChainList = new List<string>();
	[NonSerialized] public List<Vector3> _ChainPositionList = new List<Vector3>();

	private float _chainLevel = 0;
	private float _pointsGained = 0;

	[SerializeField] AnimationCurve _ChainCountdownByLevel_;
	[SerializeField] AnimationCurve _PointsGainedByLevel_;
	private float _currentCountdown;
	private float _currentTime;
	private float _countdownSpeed;
	private float _countdownSpeedSetFor;

	private bool _chainActive;

	private float _delayBetweenChains = 1.5f;
	private float _delay;

	//Specific trick actions for chain
	float _timeSinceTrick;

	private void Start () {
		ClearChain();
	}

	private void FixedUpdate () {
		_timeSinceTrick += Time.deltaTime;

		CheckSqueeze();
		CheckThreadTheNeedle();
	}

	private void Update () {

		//Somtimes countdown is set to be slower (like in scripted sections that wouldn't be fair to reset chain).
		_countdownSpeedSetFor -= Time.deltaTime;
		if (_countdownSpeedSetFor <= 0) { SetCountdownSpeed(1); }

		//Ensure can't be done immediately after ending. To allow animation to finish from ending.
		if (_delay > 0) { _delay -= Time.deltaTime; return; }

		//Countdown
		if (_currentTime <= 0) { return; }
		_currentTime -= Time.deltaTime * _countdownSpeed;
		if (_currentTime <= 0) { ClearChain(); }
	}

	#region Handling Chain

	private void ClearChain () {
		_chainActive = false;

		_pointsGained = _PointsGainedByLevel_.Evaluate(_chainLevel);
		_pointsGained = Mathf.Round(_pointsGained);

		_CoreUIElements.AChainResultText.text = _pointsGained.ToString();
		_CoreUIElements.AChainUIScript.EndChain(ExitAnimationFinished);

		_chainLevel = 0;
		_ChainList.Clear();
		_ChainPositionList.Clear();

		_delay = _delayBetweenChains;
	}

	public void AddToChain ( string source, int value, int differenceBetweenThisSourceInChain = 2, string subSource = "", float addSpeed = 0 ) {
		if (!_isActionChainEnabled || _delay >= 0) return;

		int firstIndex = _ChainList.IndexOf(source);
		int count = _ChainList.Count(s => s == source);

		//To prevent spamming the same objects
		if (subSource != "")
		{
			if (_ChainList.Contains(subSource)) { return; }
			_ChainList.Add(subSource);
		}

		//This only advances the chain if it hasn't been used too recently, or 
		bool showButDontAdvance =  count > 0 && (firstIndex + 1 < differenceBetweenThisSourceInChain) && differenceBetweenThisSourceInChain != 0;
		value = showButDontAdvance ? 0 : value;

		//Add action to list
		_CoreUIElements.AChainUIScript.SpawnNewText(source, value);

		if (!showButDontAdvance)
		{
			//UI UPDATES
			if (!_chainActive) { _CoreUIElements.AChainAnimator.SetTrigger("Enter"); _chainActive = true; }
			else
			{
				_CoreUIElements.AChainAnimator.SetTrigger("New Action");
				_CoreUIElements.AChainAnimator.SetTrigger("New Action"); //Applied twice for multiple layers.
			}

			//Add at start of list, so we can tell how long ago it was.
			_ChainList.Insert(0, source);
			_chainLevel += value;

			//Set countdown
			_currentCountdown = _ChainCountdownByLevel_.Evaluate(_chainLevel);
			_currentTime = _currentCountdown;

			//UI Countdown
			float animationLength = 2;
			_CoreUIElements.AChainAnimator.SetFloat("CountdownModifier", animationLength / _currentTime);

			//UI Level
			_CoreUIElements.AChainLevelText.text = _chainLevel.ToString();

			_Effects.ActionChainAdd(value);
			_Sounds.ActionChainSound(value);

			if(addSpeed > 0 && _PlayerVel._currentRunningSpeed > 10)
			{
				_PlayerVel.AddLateralSpeed(addSpeed);
			}
		}
	}
	#endregion

	#region managing chain UX

	public void SetCountdownSpeed ( float value ) { _countdownSpeed = value; }

	public void SetCountdownSpeedTemp ( float value, float time ) { _countdownSpeed = value; _countdownSpeedSetFor = time; }

	public void ExitAnimationFinished () {
		_CoreValues.AdjustPoints(_pointsGained);
	}
	#endregion

	#region specific actions checks

	//Check for rolling under low gaps.
	private void CheckSqueeze () {
		if(_timeSinceTrick < 0.5f) { return; }
		if(!_CoreValues._inBall || !_PlayerPhys._isGrounded) { return; }

		if(Physics.Raycast(_PlayerPhys._CharacterCenterPosition, _PlayerPhys.transform.up, out RaycastHit hit, 2, _PlayerPhys._Groundmask_))
		{
			AddToChain("Tight Squeeze", 1, 3, hit.collider.GetInstanceID().ToString());
			_timeSinceTrick = 0;
			
		}
	}

	//Check for walls or ceiling and add "trick" Actions to the chain to reward players for risking walls but remaining in control.
	private void CheckThreadTheNeedle () {
		if (_timeSinceTrick < 0.5f) { return; }

		if (_PlayerVel._coreVelocity.sqrMagnitude < 80 * 80) { return; }

		int hits = 0;

		//Get boxcast values on right first.
		Vector3 dir = _PlayerVel._worldForwardDirection;
		Vector3 boxHalfSize = new Vector3 (2.5f, (_Actions._ActionDefault._CharacterCapsule.height / 2) + 1, 0.2f);
		Vector3 startPos = _PlayerPhys._CharacterCenterPosition;

		//Ensure there is actually something to get through
		Debug.DrawRay(startPos, dir * _PlayerVel._horizontalSpeedMagnitude * Time.deltaTime * 2.5f, Color.yellow, 10f);
		if (Physics.BoxCast(startPos, new Vector3(1, 2, 0.5f), dir, Quaternion.identity, _PlayerVel._horizontalSpeedMagnitude * Time.deltaTime * 2.5f))
			return;

		startPos += _MainSkin.right * boxHalfSize.x * 1.1f;
		if(_PlayerPhys._isGrounded) startPos += _MainSkin.up * 1.1f;

		Debug.DrawRay(startPos, dir * _PlayerVel._horizontalSpeedMagnitude * Time.deltaTime * 1.5f, Color.yellow, 10f);

		//Check in front on the right or left for walls.
		if(Physics.BoxCast(startPos, boxHalfSize, dir, out RaycastHit hit, transform.rotation, _PlayerVel._horizontalSpeedMagnitude * Time.deltaTime * 1.5f, _PlayerPhys._Groundmask_))
		{
			//If this point on a wall is accessible to the player, and reflect back at them, then it is a wall that needs to be avoided.
			Debug.DrawLine(startPos, hit.point, Color.magenta, 10f);

			Vector3 reflect = Vector3.Reflect(dir, hit.normal);
			if (Vector3.Angle(reflect, -dir) < 40f) 
			{
				Debug.DrawLine(_PlayerPhys._CharacterCenterPosition, hit.point, Color.white, 10f);
				if(!Physics.Raycast(_PlayerPhys._CharacterCenterPosition, S_S_MoreMaths.GetDirection(_PlayerPhys._CharacterCenterPosition, hit.point), hit.distance * 0.95f, _PlayerPhys._Groundmask_))
					hits++;
			}

			Debug.DrawRay(hit.point, reflect * 3, Color.green, 10f);
			Debug.DrawRay(hit.point, -dir * 3, Color.red, 10f);
		}
		startPos -= _MainSkin.right * boxHalfSize.x * 1.2f * 2;
		Debug.DrawRay(startPos, dir * _PlayerVel._horizontalSpeedMagnitude * Time.deltaTime * 1.5f, Color.yellow, 10f);
		if (Physics.BoxCast(startPos, boxHalfSize, dir, out  hit, transform.rotation, _PlayerVel._horizontalSpeedMagnitude * Time.deltaTime * 1.5f, _PlayerPhys._Groundmask_))
		{
			Debug.DrawLine(startPos, hit.point, Color.magenta, 10f);

			Vector3 reflect = Vector3.Reflect(dir, hit.normal);
			if (Vector3.Angle(reflect, -dir) < 40f)
			{
				if (!Physics.Linecast(_PlayerPhys._CharacterCenterPosition, hit.point))
					hits++;
				Debug.DrawLine(_PlayerPhys._CharacterCenterPosition, hit.point, Color.white, 10f);
				if (!Physics.Raycast(_PlayerPhys._CharacterCenterPosition, S_S_MoreMaths.GetDirection(_PlayerPhys._CharacterCenterPosition, hit.point), hit.distance * 0.95f, _PlayerPhys._Groundmask_))
					hits++;
			}

			Debug.DrawRay(hit.point, reflect * 3, Color.green, 10f);
			Debug.DrawRay(hit.point, -dir * 3, Color.red, 10f);
		}

		if (hits > 0)
			StartCoroutine(WaitAndSeeIfPlayerPassesWalls( _PlayerVel._coreVelocity.sqrMagnitude, hits));

	}

	private IEnumerator WaitAndSeeIfPlayerPassesWalls (float sqrSpeedBefore, int hits) {
		for (int i = 0 ; i < 4 ; i++)
		{
			yield return new WaitForFixedUpdate();

			if(_PlayerVel._coreVelocity.sqrMagnitude < 80 * 80 || _PlayerVel._coreVelocity.sqrMagnitude < sqrSpeedBefore - 64) { yield break; } //if players down or collides, they didn't make it through.
			if (_timeSinceTrick < 0.5f) { yield break; } //if a previous check proves to have worked when waiting.
		}

		if (hits == 1)
		{
			AddToChain("Close Call", 2, 1, "", 7);
			_timeSinceTrick = 0;
		}
		else if (hits == 2)
		{
			AddToChain("Thread the Needle", 3, 1, "", 10);
			_timeSinceTrick = 0;
		}
	}

	public IEnumerator CheckLandingQuality (Vector3 groundNormal ) {
		float minSpeed = 60;

		if (_PlayerVel._totalVelocity.sqrMagnitude < minSpeed * minSpeed) { yield break; }

		Vector3 velocityBeforeLanding = _PlayerVel._totalVelocity.normalized;

		yield return new WaitForFixedUpdate();

		if (_PlayerVel._lateralVelocity.sqrMagnitude < minSpeed * minSpeed) { yield break; }

		Vector3 velocityAfterLanding = _PlayerPhys.transform.TransformDirection (_PlayerVel._lateralVelocity.normalized);

		float angle = Vector3.Angle(velocityBeforeLanding, velocityAfterLanding);

		if(angle == 0) { yield break; }

		bool goingUp = velocityAfterLanding.y > -0.1f;

		if (goingUp)
		{
			if (angle < 5)
				PerfectLanding();
			else if (angle < 10)
				GoodLanding();
		}
		else
		{
			if (angle < 13)
				PerfectLanding();
			else if (angle < 25)
				GoodLanding();
		}

		yield break;

		void PerfectLanding () {
			Debug.Log("Perfect Landing");
			AddToChain("Perfect Landing", 2, 1,"", 15);
		}

		void GoodLanding () {
			Debug.Log("Good Landing");
			AddToChain("Good Landing", 1, 2,"", 8);
		}
	}

	#endregion

	public override void AssignTools () {
		base.AssignTools();
		_Effects = _Tools.EffectsControl;
	}

	public override void AssignStats () {
		base.AssignStats();

		_PointsGainedByLevel_ = _Tools.LevelUpStats.pointsPerActionChainLevel;
		_ChainCountdownByLevel_ = _Tools.LevelUpStats.chainCountDownPerLevel;
	}

}
