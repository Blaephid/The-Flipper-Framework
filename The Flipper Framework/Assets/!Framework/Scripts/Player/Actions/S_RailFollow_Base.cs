using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_RailFollow_Base : MonoBehaviour
{
	//Current rail
	[HideInInspector] public Transform             _RailTransform;
	[HideInInspector] public CurveSample            _Sample;

	[HideInInspector]
	public Spline                 _PathSpline;
	[HideInInspector] public S_AddOnRail            _ConnectedRails;

	//ZipLine
	[HideInInspector]
	public Transform              _ZipHandle;
	[HideInInspector]
	public Rigidbody              _ZipBody;


	[HideInInspector]
	public S_Interaction_Pathers.PathTypes _whatKindOfRail;     //Set when entering the action, deciding if this is a zipline, rail or later added type


	[HideInInspector] public bool                   _isGoingBackwards;  //Is the player going up or down on the spline points.
	[HideInInspector] public int                   _movingDirection;   //A 1 or -1 based on going backwards or not. Used in calculations.

	[HideInInspector]
	public float                  _pointOnSpline = 0f; //The actual place on the spline being travelled. The number is how many units along the length of the spline it is (not affected by spline length).
	private float                  _clampedPointOnSpline = 0f;

	[HideInInspector] public Spline.SampleTransforms                        _sampleTransforms;
	[HideInInspector] public Vector3               _sampleForwards;    //The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.
	[HideInInspector] public Vector3               _sampleRight;
	[HideInInspector] public Vector3               _sampleLocation;
	[HideInInspector] public Vector3               _sampleUpwards;    //The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.

	[HideInInspector] public float       _grindingSpeed;     //Set by action pathSpeeds every frame. Used to check movement along rail.

	//Quaternion rot;
	[HideInInspector] public Vector3               _setOffSet;         //Will follow a spline at this distance (relevant to sample forwards). Set when entering a spline and used to grind on rails offset of the spline. Hopping will change this value to move to the sides.
	[HideInInspector] public float                 _upOffsetRail_ = 2.05f;
	[HideInInspector] public float                 _upOffsetZip_ = -2.05f;

	[HideInInspector] public bool                  _isRailLost;        //Set to true when point on spline surpasses the limit, to inform later if statements that update.

	private float _distanceMovedSinceLastFixedUpdate;
	private float _timeAtLastFixedUpdate;

	public void CustomUpdate () {

		//Amount along rail to travel is based on how long it's been since the last Update OR FixedUpdate call.
		float travelAmount = Time.deltaTime * _grindingSpeed;

		if (_distanceMovedSinceLastFixedUpdate == 0) //If last move was a fixedUpdate, then go from that
		{
			float timeSinceLastFixedUpdate = Time.time - _timeAtLastFixedUpdate;
			travelAmount = timeSinceLastFixedUpdate * _grindingSpeed;
		}

		_distanceMovedSinceLastFixedUpdate += travelAmount;

		//Prevents player from moving more than one FixedUpdates worth across mutliple Updates.
		if (_distanceMovedSinceLastFixedUpdate > Time.fixedDeltaTime * _grindingSpeed)
		{
			float limit = Time.fixedDeltaTime * _grindingSpeed;
			travelAmount = _distanceMovedSinceLastFixedUpdate - limit;
			_distanceMovedSinceLastFixedUpdate = limit;
		}

		GetNewSampleOnRail(travelAmount);
	}

	public void CustomFixedUpdate () {
		float travelAmount = Time.fixedDeltaTime * _grindingSpeed;
		travelAmount -= _distanceMovedSinceLastFixedUpdate;

		_timeAtLastFixedUpdate = Time.time;

		_distanceMovedSinceLastFixedUpdate = 0;
		GetNewSampleOnRail(travelAmount);
	}

	public void StartOnRail () {
		_timeAtLastFixedUpdate = Time.time;
		_distanceMovedSinceLastFixedUpdate = 0;
	}

	public void GetNewSampleOnRail ( float travelAmount ) {

		if(!_RailTransform || !_PathSpline) { return; }

		//Increase/decrease the Amount of distance travelled on the Spline by DeltaTime and direction
		_movingDirection = _isGoingBackwards ? -1 : 1;
		_pointOnSpline += travelAmount * _movingDirection;

		_isRailLost = _pointOnSpline < 0 || _pointOnSpline > _PathSpline.Length;
		_clampedPointOnSpline = _pointOnSpline;

		if (_isRailLost)
		{
			if (CheckLoseRail())
				_clampedPointOnSpline = Mathf.Clamp(_pointOnSpline, 0, _PathSpline.Length);
			else
				_clampedPointOnSpline = _pointOnSpline;
		}

		//Get the data of the spline at that point along it (rotation, location, etc)
		_Sample = _PathSpline.GetSampleAtDistance(_clampedPointOnSpline);

		_sampleTransforms = Spline.GetSampleTransformInfo(_RailTransform, _Sample);
		_sampleForwards = _sampleTransforms.forwards * _movingDirection;
		_sampleUpwards = _sampleTransforms.upwards;
		_sampleRight = _sampleTransforms.right;
		_sampleLocation = _sampleTransforms.location;
	}

	//Physics
	//Gets new location on rail, changing position and rotation to match. Called in update in order to ensure the character matches the rail in real time.
	public void PlaceOnRail ( Action<S_Interaction_Pathers.PathTypes> SetRotationOnRail, Action<Vector3> SetPosition ) {
		if(!_RailTransform) { return; }

		//Set Position and rotation on Rail
		switch (_whatKindOfRail)
		{
			//Place character in world space on point in rail
			case S_Interaction_Pathers.PathTypes.rail:
				Vector3 relativeOffset = _RailTransform.rotation * _Sample.Rotation * -_setOffSet; //Moves player to the left or right of the spline to be on the correct rail

				//Position is set to the local location of the spline point, the location of the spline object, the player offset relative to the up position (so they're actually on the rail) and the local offset.
				Vector3 newPos = _sampleLocation;
				newPos += (_sampleUpwards * _upOffsetRail_) + relativeOffset;
				SetPosition(newPos);
				break;

			case S_Interaction_Pathers.PathTypes.zipline:

				//Similar to on rail, but place handle first, and player relevant to that.
				newPos = _sampleLocation;
				newPos += _setOffSet;
				_ZipHandle.transform.position = newPos;
				SetPosition(newPos + (_ZipHandle.transform.up * _upOffsetZip_));
				break;
		}

		SetRotationOnRail.Invoke(_whatKindOfRail);
	}


	//Checks the properties of the rail to see if should enter 
	public bool CheckLoseRail () {

		//If the spline loops around then just move place on length back to the start or end.
		if (_PathSpline.IsLoop)
		{
			_pointOnSpline += _PathSpline.Length * -_movingDirection;
			_isRailLost = false;
			return false;
		}

		//Or if this rail has either a next rail or previous rail attached.
		else if (_ConnectedRails != null)
		{

			//If going forwards, and the rail has a rail off the end, then go onto it.
			if (!_isGoingBackwards && _ConnectedRails.UseNextRail != null && _ConnectedRails.UseNextRail.isActiveAndEnabled)
			{
				//Set point on spline to be how much over this grind went over the current rail.
				_pointOnSpline = Mathf.Max(0, _pointOnSpline - _PathSpline.Length);

				//The data storing next and previous rails is changed to the one for the new rail, meaning this rail will now become UsePrevRail.
				SetNewRail(_ConnectedRails.UseNextRail);
				return false;
			}
			//If going backwards, and the rail has a rail off the end, then go onto it.
			else if (_isGoingBackwards && _ConnectedRails.UsePrevRail != null && _ConnectedRails.UsePrevRail.isActiveAndEnabled)
			{
				//Set data first, because will need to affect point by new length.
				SetNewRail(_ConnectedRails.UsePrevRail);

				//Since coming onto this new rail from the end of it, must have a reference to its length. This is why the point is acquired at the end of this if flow, rather than the start.
				_pointOnSpline = _pointOnSpline + _PathSpline.Length;
				return false;
			}

			void SetNewRail ( S_AddOnRail NewRail ) {
				_isRailLost = false;

				_ConnectedRails = NewRail;
				//Change the offset to match this rail (since may go from a rail offset from a spline, straight onto rail directily on a different spline)
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>()._mainOffset.x, 0, 0);

				//Set path and positions to follow.
				_PathSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _PathSpline.transform.parent;
			}
		}
		//If hasn't returned yet, then there is nothing to follow, so actually leave the rail.

		return true;
	}

	//Goes through whole spline and returns the point closests to the given position, along with how far it is.
	public static Vector2 GetClosestPointOfSpline ( Vector3 colliderPosition, Spline thisSpline, Vector3 offset, float incrementsToIncrease = 5 ) {
		float ClosestDistanceSquared = 9999999f;
		float closestSample = 0;
		if (!thisSpline || !thisSpline.transform) { return Vector2.zero; }

		for (float n = 0 ; n < thisSpline.Length ; n += incrementsToIncrease)
		{
			n = Mathf.Min(n, thisSpline.Length);

			CurveSample splineSample = thisSpline.GetSampleAtDistance(n);
			Spline.SampleTransforms sampleTransform = Spline.GetSampleTransformInfo(thisSpline.transform, splineSample);

			//Place on spline relative to object rotation and offset.
			Vector3 checkPos = sampleTransform.location + (splineSample.Rotation * offset);

			//The distance between the point at distance n along the spline, and the current collider position.
			float distanceSquared = S_S_MoreMaths.GetDistanceSqrOfVectors(checkPos,colliderPosition);

			//Every time the distance is lower, the closest sample is set as that, so by the end of the loop, this will be set to the closest point.
			if (distanceSquared <= ClosestDistanceSquared)
			{
				ClosestDistanceSquared = distanceSquared;
				closestSample = n;
			}

			if (distanceSquared < 1) { break; }
			if (ClosestDistanceSquared < incrementsToIncrease * incrementsToIncrease && distanceSquared > 500 * 500)
			{ break; }
		}
		return new Vector2(closestSample, ClosestDistanceSquared);
	}
}
