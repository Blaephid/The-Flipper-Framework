using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class S_Data_Spring : S_Data_Base
{
	[Header("Bounce")]
	[Tooltip("The magnitude of the environmental velocity added to player.")]
	public float _springForce_;
	[Tooltip("If false, player will always bounce in the spring direction, at its set force. If true, this is more maluable, and the player's launch direction will be affected by horizontal velocity before hitting spring.")]
	public bool _keepHorizontal_;

	[Header("Objects")]
	[Tooltip("This transform is used for bounce direction and position player is set to before the launch..")]
	public Transform _BounceTransform;
	[Tooltip("The spring animator, will be triggered when used.")]
	public Animator _Animator { get; set; }

	[Header ("On Player")]
	[Tooltip("If not null, the player will face in the given direction, even if not being launched that way. Useful on springs straight up.")]
	public Transform _SetPlayerForwardsTo_;
	[Tooltip("If set to true, player will not be able to change input after launched.")]
	public bool _willLockControl_ = false;
	[Tooltip("How amy frames until the player regains control if the above is true.")]
	public int _lockForFrames_ = 60;
	[Tooltip("What the player's input will be during the frames their control is locked.")]
	public S_GeneralEnums.LockControlDirection _LockInputTo_;

	[Tooltip("Since character's can have different gravities. If this is not zero, the player gravity will be this until they hit the ground.")]
	public Vector3 _overwriteGravity_;
	[Tooltip("The player will not be able to perform air actions (homing, jump dash, bounce, double jump) until this many frames after being launched.")]
	public float _lockAirMovesTime_ = 30f;

	void Start () {
		_Animator = GetComponent<Animator>();
	}
}
