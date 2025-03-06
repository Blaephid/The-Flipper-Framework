using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Data Components/Spring")]
public class S_Data_Spring : S_Data_Base
{
	[Header("Bounce")]
	public S_Structs.LaunchPlayerData _launchData_;
	[Tooltip("If false, player will always bounce in the spring direction, at its set force. If true, this is more maluable, and the player's launch direction will be affected by horizontal velocity before hitting spring.")]
	public bool _keepHorizontal_;

	[Header("Objects")]
	[Tooltip("The spring animator, will be triggered when used.")]
	public Animator _Animator { get; set; }

	[Header ("On Player")]
	[Tooltip("If true, on launch, the player will be set to face the forward direction of the spring, but keeping their head upwards.")]
	public bool _changePlayerForwards;


	void Start () {
		_Animator = GetComponent<Animator>();
	}

	[ExecuteInEditMode]
	void Update () {
		if (Application.isPlaying) { return; }
		UpdateLaunchDataToDirection();
	}

	private void OnEnable () {
		UpdateLaunchDataToDirection();
	}

	private void UpdateLaunchDataToDirection () {
		_launchData_ = new S_Structs.LaunchPlayerData()
		{
			_force_ = _launchData_._force_,
			_direction_ = _launchData_._direction_,
			_directionToUse_ = (transform.rotation * _launchData_._direction_).normalized,
			_lockInputFrames_ = _launchData_._lockInputFrames_,
			_lockAirMovesFrames_ = _launchData_._lockAirMovesFrames_,
			_overwriteGravity_ = _launchData_._overwriteGravity_,
			_lockInputTo_ = _launchData_._lockInputTo_
		};
	}
}
