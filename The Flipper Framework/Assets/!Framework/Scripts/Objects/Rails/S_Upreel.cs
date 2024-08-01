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

	[Header("Visuals and effects")]
	public LineRenderer			_Line;


	//Stats
	public bool	_isDeployedOnStart = true;
	public float	_reelSpeed;
	public float	_maxLength;

	// Trackers
	public bool		_isMoving { get; protected set; }
	[Range(0, 1)] 
	public float		_currentLengthPercentage = 1;

	#endregion


	void Start () {
		//Checks if is playing because this updates in editor as well
		if (Application.isPlaying && _isDeployedOnStart)
		{
			DeployOrRetractHandle(true);
		}
	}

	// Also called in editor mode.
	void Update () {
		if (!_HandleObject) return; //Only execute if assigned to avoid null pointers

		//Set handle object to place in scene corresponding to current line length
		Vector3 position = _HandleObject.transform.localPosition;
		position.y = -_maxLength * _currentLengthPercentage;
		_HandleObject.transform.localPosition = position;

		//Ensures two points of line are between handle and base.
		if (!_Line || !_TopAchor || !_LineEndAnchor) return;
		_Line.SetPosition(0, _TopAchor.position);
		_Line.SetPosition(1, _LineEndAnchor.position);
	}

	//Called when the handle is going down.
	public void DeployOrRetractHandle (bool isDeploying) {
		_isMoving = true;
		_AudioSource.Play();

		//If retracting, ensure collision is disabled to avoid strange interacitons.
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
			_currentLengthPercentage = Mathf.MoveTowards(_currentLengthPercentage, targetExtentPercentage, Time.deltaTime * _reelSpeed);

			yield return null; //Waits until next update.
		}

		_AudioSource.Stop();
		_isMoving = false;

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

		////If extending
		//if (targetExtentPercentage > _currentLengthPercentage)
		//{
		//	while (_currentLengthPercentage < targetExtentPercentage)
		//	{
		//		_currentLengthPercentage += Time.deltaTime * _reelSpeed;
		//		yield return null;
		//		if (_currentLengthPercentage >= targetExtentPercentage)
		//		{
		//			_currentLengthPercentage = targetExtentPercentage;
		//			_AudioSource.Stop();
		//			_isMoving = false;
		//			_HandleObject.GetComponent<Collider>().enabled = true;
		//			_HomingTarget.SetActive(true);
		//			break;
		//		}
		//	}
		//}

		////If Retracting
		//else if (targetExtentPercentage < _currentLengthPercentage)
		//{
		//	while (_currentLengthPercentage > targetExtentPercentage)
		//	{
		//		_currentLengthPercentage -= Time.deltaTime * _reelSpeed;
		//		yield return null;
		//		if (_currentLengthPercentage <= targetExtentPercentage)
		//		{
		//			_currentLengthPercentage = targetExtentPercentage;
		//			_AudioSource.Stop();
		//			_isMoving = false;
		//			StartCoroutine(ResetPulley(2f));
		//			break;
		//		}
		//	}
		//}
	}

	IEnumerator ResetHandleAfterDelay ( float delay ) {
		yield return new WaitForSeconds(delay);
		DeployOrRetractHandle(true);
	}
}
