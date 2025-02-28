using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
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

	[HideInInspector] public Vector3               _sampleForwards;    //The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.
	[HideInInspector] public Vector3               _sampleLocation;
	[HideInInspector] public Vector3               _sampleUpwards;    //The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.

	[HideInInspector] public float       _grindingSpeed;     //Set by action pathSpeeds every frame. Used to check movement along rail.

	//Quaternion rot;
	[HideInInspector] public Vector3               _setOffSet;         //Will follow a spline at this distance (relevant to sample forwards). Set when entering a spline and used to grind on rails offset of the spline. Hopping will change this value to move to the sides.
	private float                 _offsetRail_ = 2.05f;
	private float                 _offsetZip_ = -2.05f;

	[HideInInspector] public bool                  _isRailLost;        //Set to true when point on spline surpasses the limit, to inform later if statements that update.


	//Physics
	//Gets new location on rail, changing position and rotation to match. Called in update in order to ensure the character matches the rail in real time.
	public void PlaceOnRail (Action<S_Interaction_Pathers.PathTypes> SetRotationOnRail, Action<Vector3> SetPosition) {
		//Increase/decrease the Amount of distance travelled on the Spline by DeltaTime and direction
		float travelAmount = (Time.deltaTime * _grindingSpeed);
		_movingDirection = _isGoingBackwards ? -1 : 1;

		_pointOnSpline += travelAmount * _movingDirection;
		_isRailLost = _pointOnSpline < 0 || _pointOnSpline > _PathSpline.Length;
		float clampedPoint = Mathf.Clamp(_pointOnSpline, 0, _PathSpline.Length - 1);

		//Get the data of the spline at that point along it (rotation, location, etc)
		_Sample = _PathSpline.GetSampleAtDistance(clampedPoint);
		_sampleForwards = _RailTransform.rotation * _Sample.tangent * _movingDirection;
		_sampleUpwards = (_RailTransform.rotation * _Sample.up);
		_sampleLocation = _RailTransform.position + (_RailTransform.rotation * _Sample.location);

		//Set player Position and rotation on Rail
		switch (_whatKindOfRail)
		{
			//Place character in world space on point in rail
			case S_Interaction_Pathers.PathTypes.rail:

				SetRotationOnRail.Invoke(_whatKindOfRail);
				Vector3 relativeOffset = _RailTransform.rotation * _Sample.Rotation * -_setOffSet; //Moves player to the left or right of the spline to be on the correct rail

				//Position is set to the local location of the spline point, the location of the spline object, the player offset relative to the up position (so they're actually on the rail) and the local offset.
				Vector3 newPos = _sampleLocation;
				newPos += (_sampleUpwards * _offsetRail_) + relativeOffset;
				SetPosition(newPos);
				break;

			case S_Interaction_Pathers.PathTypes.zipline:

				SetRotationOnRail.Invoke(_whatKindOfRail);

				//Similar to on rail, but place handle first, and player relevant to that.
				newPos = _sampleLocation;
				newPos += _setOffSet;
				_ZipHandle.transform.position = newPos;
				SetPosition(newPos + (_ZipHandle.transform.up * _offsetZip_));
				break;
		}

	}


	//Checks the properties of the rail to see if should enter 
	public void CheckLoseRail (Action LoseRail) {

		//If the spline loops around then just move place on length back to the start or end.
		if (_PathSpline.IsLoop)
		{
			_pointOnSpline = _pointOnSpline + (_PathSpline.Length * -_movingDirection);
			_isRailLost = false;
			return;
		}

		//Or if this rail has either a next rail or previous rail attached.
		else if (_ConnectedRails != null)
		{
			_isRailLost = false;

			//If going forwards, and the rail has a rail off the end, then go onto it.
			if (!_isGoingBackwards && _ConnectedRails.nextRail != null && _ConnectedRails.nextRail.isActiveAndEnabled)
			{
				//Set point on spline to be how much over this grind went over the current rail.
				_pointOnSpline = Mathf.Max(0, _pointOnSpline - _PathSpline.Length);

				//The data storing next and previous rails is changed to the one for the new rail, meaning this rail will now become PrevRail.
				_ConnectedRails = _ConnectedRails.nextRail;

				//Change the offset to match this rail (since may go from a rail offset from a spline, straight onto rail directily on a different spline)
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>()._offset3d_.x, 0, 0);

				//Set path and positions to follow.
				_PathSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _PathSpline.transform.parent;
				return;
			}
			//If going backwards, and the rail has a rail off the end, then go onto it.
			else if (_isGoingBackwards && _ConnectedRails.PrevRail != null && _ConnectedRails.PrevRail.isActiveAndEnabled)
			{
				//Set data first, because will need to affect point by new length.
				_ConnectedRails = _ConnectedRails.PrevRail;

				// Change offset to match the new rail.
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>()._offset3d_.x, 0, 0);

				//Set path and positions to follow.
				_PathSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _PathSpline.transform.parent;

				//Since coming onto this new rail from the end of it, must have a reference to its length. This is why the point is acquired at the end of this if flow, rather than the start.
				_pointOnSpline = _pointOnSpline + _PathSpline.Length;
				_pointOnSpline = _PathSpline.Length;
				return;
			}
		}
		//If hasn't returned yet, then there is nothing to follow, so actually leave the rail.

		LoseRail();
	}
}
