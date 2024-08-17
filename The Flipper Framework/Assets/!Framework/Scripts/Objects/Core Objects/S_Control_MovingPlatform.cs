using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class S_Control_MovingPlatform : MonoBehaviour
{
	[Header("Projection")]
	public bool         _projectPath = true;
	public int          _calculations = 500;

	[Header("In Game")]
	public bool         _isActive = true;
	private float       _counterForMove;

	public List<strucMove> _ListOfMovesOnStart = new List<strucMove>() { new strucMove {
		_XSpeedByTime_ = new AnimationCurve(new Keyframe[] {
		new Keyframe(0f,1f),
		new Keyframe(0.8f,1f),
		new Keyframe(1f,0.7f) }),
		_YSpeedByTime_ = new AnimationCurve(new Keyframe[] {
		new Keyframe(0f,1f),
		new Keyframe(0.8f,1f),
		new Keyframe(1f,0.7f) }),
		_ZSpeedByTime_  = new AnimationCurve(new Keyframe[] {
		new Keyframe(0f,1f),
		new Keyframe(0.8f,1f),
		new Keyframe(1f,0.7f) }),
		_XMaxDistance_ = 10,
		_XSpeed_ = 5,
		_ZMaxDistance_ = 10,
		_ZSpeed_ = 5,
		_YSpeed_ = 5,
		_willReturnOnX = true,
		_willReturnOnY = true,
		_willReturnOnZ = true,
} };

	private List<strucMove> _ListOfMovesForSimulation = new List<strucMove>();

	[Serializable]
	public struct strucMove
	{

		[Header("X")]
		public AnimationCurve         _XSpeedByTime_;
		public float                  _XMaxDistance_;
		public float                  _XSpeed_ ;
		public bool                   _willReturnOnX;
		public float                  _XDelay;

		[Header("Z")]
		public AnimationCurve         _ZSpeedByTime_;
		public float                  _ZMaxDistance_;
		public float                  _ZSpeed_;
		public bool                   _willReturnOnZ;
		public float                  _ZDelay;


		[Header ("Y")]
		public AnimationCurve         _YSpeedByTime_;
		public float                  _YMaxDistance_ ;
		public float                  _YSpeed_;
		public bool                   _willReturnOnY;
		public float                  _YDelay;
	}


	public Vector3 _currentPosition { get; set; }
	public Vector3 TranslateVector { get; set; }


	Vector3 _startPosition;


	void Start () {
		_startPosition = transform.position;
		_currentPosition = transform.position;

		SetPlayListAsStart();
	}

	private void Update () {
		if (_isActive)
		{
			transform.position = MovePlatform(0, Time.deltaTime);
		}
	}

	private Vector3 MovePlatform ( int move, float timeStep ) {

		strucMove currentMove = _ListOfMovesForSimulation[move];

		//Gets the current distance the platform currently is from the starting points on each axis.
		float Xpos = _currentPosition.x - _startPosition.x;
		float Zpos = _currentPosition.z -  _startPosition.z;
		float Ypos = _currentPosition.y - _startPosition.y ;

		_counterForMove += timeStep;

		//For each axis, will do the same, but with different variables.

		//If currently set able to move on this axis. And delay has worn off.
		if (currentMove._XSpeed_ > 0 && Mathf.Abs(currentMove._XMaxDistance_) > 1 && _counterForMove > currentMove._XDelay)
		{
			//Get a distance to move this frame with speed, affected by percentage of journey complete.
			float xMove = currentMove._XSpeed_ * currentMove._XSpeedByTime_.Evaluate(Mathf.Abs(Xpos) / currentMove._XMaxDistance_);

			//Use this distance to move towards the max distance.
			Xpos = Mathf.MoveTowards(Xpos, currentMove._XMaxDistance_, xMove * timeStep);

			//If reaches this, change direction.
			if (Xpos == currentMove._XMaxDistance_ && currentMove._willReturnOnX) { currentMove._XMaxDistance_ = -currentMove._XMaxDistance_; }
		}

		//If currently set able to move on this axis.
		if (currentMove._ZSpeed_ > 0 && Mathf.Abs(currentMove._ZMaxDistance_) > 1 && _counterForMove > currentMove._ZDelay)
		{
			//Get a distance to move this frame with speed, affected by percentage of journey complete.
			float ZMove = currentMove._ZSpeed_ * currentMove._ZSpeedByTime_.Evaluate(Mathf.Abs(Zpos) / currentMove._ZMaxDistance_);

			//Use this distance to move towards the max distance.
			Zpos = Mathf.MoveTowards(Zpos, currentMove._ZMaxDistance_, ZMove * timeStep);

			//If reaches this, change direction.
			if (Zpos == currentMove._ZMaxDistance_ && currentMove._willReturnOnZ) { currentMove._ZMaxDistance_ = -currentMove._ZMaxDistance_; }
		}

		//If currently set able to move on this axis.
		if (currentMove._YSpeed_ > 0 && Mathf.Abs(currentMove._YMaxDistance_) > 1 && _counterForMove > currentMove._YDelay)
		{
			//Get a distance to move this frame with speed, affected by percentage of journey complete.
			float YMove = currentMove._YSpeed_ * currentMove._YSpeedByTime_.Evaluate(Mathf.Abs(Ypos) / currentMove._YMaxDistance_);

			//Use this distance to move towards the max distance.
			Ypos = Mathf.MoveTowards(Ypos, currentMove._YMaxDistance_, YMove * timeStep);

			//If reaches this, change direction.
			if (Ypos == currentMove._YMaxDistance_ && currentMove._willReturnOnY) { currentMove._YMaxDistance_ = -currentMove._YMaxDistance_; }
		}

		_ListOfMovesForSimulation[move] = currentMove; //Apply any changes made to direction / distance.

		//Bring all of these changes on each axis back in, as an offset from the start position.
		_currentPosition = new Vector3(Xpos + _startPosition.x, Ypos + _startPosition.y, Zpos + _startPosition.z);
		return _currentPosition;
	}

	//To allow simulation to run without affecting move lists (because distance are inverted on return), use a copy list of moves so the original isn't changed every draw.
	private void SetPlayListAsStart () {
		_ListOfMovesForSimulation.Clear();
		for (int i = 0 ; i < _ListOfMovesOnStart.Count ; i++)
		{
			_ListOfMovesForSimulation.Add(_ListOfMovesOnStart[i]);
		}
	}

#if UNITY_EDITOR
	//If selecting this instance in editor outside of play mode, will make all the movement calculations in order (up to a limit), and draw lines, in order to show the path the object will take from this script.
	private void OnDrawGizmosSelected () {
		if (!Application.isPlaying)
		{
			//For tracking movements
			_startPosition = transform.position;
			_currentPosition = transform.position;

			//To prevent the simulations from affecting the actual movements, make sure it starts from the same stats each time.
			SetPlayListAsStart();

			//For each calculation, get the previous position, apply a movement, then draw a line from the former to the latter.
			for (int i = 0 ; i < _calculations ; i++)
			{
				Vector3 lineStart = _currentPosition;
				MovePlatform(0, Time.fixedDeltaTime);

				Gizmos.color = Color.red;
				Gizmos.DrawLine(lineStart, _currentPosition);
			}
		}
	}
#endif
}
