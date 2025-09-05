using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Trigger_MusicChange : S_Trigger_Base, ITriggerable
{

	[SerializeField] private AudioClip SongToSwapInto;
	private AudioClip OriginalTrack;
	private AudioSource _MusicSource;
	[SerializeField] private bool Toggle;
	[SerializeField] private bool ShouldHappenOnlyOnce;
	[SerializeField] private float startPoint = 0;

	public void OnTriggerEnter ( Collider col ) {
		if (col.tag == "Player")
		{
			_MusicSource = col.GetComponentInParent<S_PlayerCoreValues>()._Music;


			if (!Toggle)
			{
				Toggle = !Toggle;
				OriginalTrack = _MusicSource.clip;
				_MusicSource.clip = SongToSwapInto;
				_MusicSource.time = startPoint;
				_MusicSource.Play();
				SongToSwapInto = OriginalTrack;
			}


			//Debug.Log ("Music is now: "+Source.clip.name);

			if (!ShouldHappenOnlyOnce)
			{
				Toggle = !Toggle;
			}

		}
	}
}

