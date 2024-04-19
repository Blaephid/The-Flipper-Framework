using UnityEngine;
using System.Collections;

public class S_MovingRing : MonoBehaviour
{

	public float _collectTime_ = 0.6f;
	float _counter;
	public bool _isCollectable { get; set; }
	float _flickCount = 0;

	public float _duration_ = 6;

	void Start () {
		_isCollectable = false;
	}

	void Update () {

		_counter += Time.deltaTime;
		if (_counter > _collectTime_)
		{
			_isCollectable = true;
		}
		if (_counter > _duration_ - 2)
		{
			RingFlicker();
		}
		if (_counter > _duration_)
		{
			Destroy(gameObject);
		}

	}

	public void RingFlicker () {
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

}
