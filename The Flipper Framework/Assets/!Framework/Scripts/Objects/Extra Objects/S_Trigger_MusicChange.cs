using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Trigger_MusicChange : MonoBehaviour {

	[SerializeField] private AudioClip SongToSwapInto;
	private AudioClip OriginalTrack;
	[SerializeField] private AudioSource Source;
	[SerializeField] private bool Toggle;
	[SerializeField] private bool ShouldHappenOnlyOnce;
	[SerializeField] private float startPoint = 0;

	public void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player") {

			if (!Toggle) 
			{
				Toggle = !Toggle;
				OriginalTrack = Source.clip;
				Source.clip = SongToSwapInto;
				Source.time = startPoint;
				Source.Play ();
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

