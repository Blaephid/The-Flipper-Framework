using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class S_Upreel : MonoBehaviour
{
	
	

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	public GameObject			_HomingTarget;
	public GameObject			_HandleObject;
	public Transform			_HandlePosition;
	public Transform			_TopAchor;
	public Transform			_LineEndAnchor;
	public AudioSource			_AudioSource;
	public Rigidbody                        _HandleRB;

	[Header("Visuals and effects")]
	public LineRenderer			_Line;


	//Stats
	public bool	_isDeployedOnStart = true;
	public float	_timeToReachTop = 1;
	public float	_maxLength;
	public float        _launchUpwardsForce = 90;

	// Trackers
	public bool		_isMoving { get; protected set; }
	[Range(0, 1)] 
	public float		_currentLengthPercentage = 1;
	[HideInInspector]
	public bool                   _isPlayerOn = false;
	[HideInInspector]
	public Vector3                _velocity;
	private float                 _worldSpeed;
	private int                   _direction = -1;

	#endregion


	void Start () {
		//Checks if is playing because this updates in editor as well
		if (Application.isPlaying)
		{
			if (_isDeployedOnStart)
				DeployOrRetractHandle(true);

			_worldSpeed = _maxLength / _timeToReachTop;
		}
	}

	// Also called in editor mode.
	void Update () {

		if (!Application.isPlaying || _direction == -1)
		{
			PlaceHandleOnLength();
		}
			SetLine();
		
	}

	public Vector3 PlaceHandleOnLength () {
		if (!_HandleObject) return Vector3.zero; //Only execute if assigned. to avoid null pointers

		//Set handle object to place in scene corresponding to current line length
		Vector3 position = _TopAchor.position - (transform.up * _maxLength * _currentLengthPercentage);
		_HandleObject.transform.position = position;
		return position;
	}


	//Called on the fixedUpdate of the player action. Using velocity to ensure it's smooth.
	public Vector3 MoveHandleToLength () {

		if (_isMoving) //Because this will also be called the frame the player leaves the upreel.
		{
			_velocity = _direction * _worldSpeed * transform.up;
			_HandleRB.velocity = _velocity;
		}

		//Send the location back to the player.
		return PlaceHandleOnLength();
	}

	public void SetLine () {
		//Ensures two points of line are between handle and base.
		if (!_Line || !_TopAchor || !_LineEndAnchor) return;
		_Line.SetPosition(0, _TopAchor.position);
		_Line.SetPosition(1, _LineEndAnchor.position);

	}

	//Called when the handle is going down.
	public void DeployOrRetractHandle (bool isDeploying) {
		_isMoving = true;
		_AudioSource.Play();

		_isPlayerOn = !isDeploying;
		_direction = isDeploying ? -1 : 1;

		//If retracting, ensure collision is disabled to avoid strange interactions.
		if (!isDeploying)
		{
			_HandleObject.GetComponent<Collider>().enabled = false;
			_HomingTarget.SetActive(false);
		}

		//Call coroutine to move handle to full extent or base.
		StartCoroutine(SetHandlePosition(isDeploying ? 1 : 0));
	}

	IEnumerator SetHandlePosition ( float targetExtentPercentage ) {

		while (_currentLengthPercentage != targetExtentPercentage)
		{
			//Increase or decrease length of pulley towards target length. When they are equal, the loop will end.
			_currentLengthPercentage = Mathf.MoveTowards(_currentLengthPercentage, targetExtentPercentage, Time.fixedDeltaTime / _timeToReachTop);

			yield return new WaitForFixedUpdate(); //Waits until next update.
		}

		PlaceHandleOnLength(); //Called again here (not just in Update) to ensure at the right point before a player launches off.
		_HandleRB.velocity = Vector3.zero;
		_AudioSource.Stop();
		_isMoving = false;
		_direction = 0;

		//If now fully extended
		if(targetExtentPercentage == 1)
		{
			_HandleObject.GetComponent<Collider>().enabled = true;
			_HomingTarget.SetActive(true);
		}
		//If now fully retracted
		else if (targetExtentPercentage == 0)
		{
			StartCoroutine(ResetHandleAfterDelay(2f));
		}
	}

	IEnumerator ResetHandleAfterDelay ( float delay ) {
		yield return new WaitForSeconds(delay);
		DeployOrRetractHandle(true);
	}
}
