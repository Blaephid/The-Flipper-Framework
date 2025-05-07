using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CustomGravity : MonoBehaviour
{
	[CustomReadOnly, SerializeField] Rigidbody _RB;
	[BaseColour(0.8f, 0.8f, 0.8f, 1f)] public bool _isGravityOn;
	[BaseColour(0.8f, 0.8f, 0.8f, 1f), SerializeField] LayerMask _GroundMask;

	[Header("Values")]
	[Tooltip("Gain this speed downwards every FixedUpdate while Gravity is active")]
	[SerializeField] float _gravity;
	[Tooltip("Gravity will not be applied if falling faster than this.")]
	[SerializeField] float _maxNaturalFallSpeed = 100;
	[Tooltip("Can never fall faster than this.")]
	[SerializeField] float _maxFallSpeed = 200;
	[Tooltip("This is subtracted hrom horizontal velocity every FixedUpdate.")]
	[SerializeField] float _horizontalDrag;

	[SerializeField] private AnimationCurve _StartingDragAndGravity = new AnimationCurve (new Keyframe[] { new Keyframe (0, 0), new Keyframe (1.5f, 1) });

	private float _currentFallSpeed;
	private int _framesInAir;

	private void Start () {
		_RB.useGravity = false;
	}

	private void OnValidate () {
		_RB = GetComponent<Rigidbody>();
	}

	private void FixedUpdate () {
		if (_isGravityOn)
		{
			_framesInAir++;
			float thisFrame = _StartingDragAndGravity.Evaluate((float)_framesInAir * Time.fixedDeltaTime);


			_currentFallSpeed = Mathf.Abs(_RB.velocity.y);
			if (Physics.Raycast(transform.position, Vector3.down, _currentFallSpeed * Time.fixedDeltaTime, _GroundMask)) _isGravityOn = false;

			if(_currentFallSpeed <= _maxNaturalFallSpeed) 
				_RB.velocity += Vector3.down * _gravity *thisFrame;

			Vector3 horizVelocity = new Vector3(_RB.velocity.x,0, _RB.velocity.z);
			horizVelocity = Vector3.MoveTowards(horizVelocity, Vector3.zero, _horizontalDrag * thisFrame);

 			_RB.velocity.Set(horizVelocity.x, Mathf.Max(_RB.velocity.y, -_maxFallSpeed), horizVelocity.z);
		}
	}

	public void SetGravityOn(bool set ) {
		if(set != _isGravityOn)
		{
			_isGravityOn = set;
			_framesInAir = 0;
		}
	}

}
