using UnityEngine;
using System.Collections;

public class S_MovingRing : MonoBehaviour
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Stats
	#region Stats
	[Tooltip("How many seconds after being spawned until the ring can be picked up.")]
	public float _collectTime_ = 0.6f;
	[Tooltip("How many seconds until the rings despawn..")]
	public float _duration_ = 6;
	#endregion

	// Trackers
	#region trackers
	private float _counter;
	public bool _isCollectable { get; set; }
	private float _flickCount = 0;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	void Start () {
		_isCollectable = false;
	}

	void Update () {

		_counter += Time.deltaTime;
		//Checks when it can be picked up.
		if (_counter > _collectTime_)
		{
			_isCollectable = true;
		}
		//Starts the flickering to show about despawn.
		if (_counter > _duration_ - 2)
		{
			RingFlicker();
		}
		//Despawn after duration.
		if (_counter > _duration_)
		{
			Destroy(gameObject);
		}

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//On a timer, temporarily hides and unhides the renderer for the visual.
	private void RingFlicker () {
		_flickCount += Time.deltaTime * 180;
		if (_flickCount < 0)
		{
			gameObject.GetComponent<MeshRenderer>().enabled = false;
		}
		else
		{
			gameObject.GetComponent<MeshRenderer>().enabled = true;
		}
		if (_flickCount > 10)
		{
			_flickCount = -10;
		}
	}
	#endregion

}
