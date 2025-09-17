using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AudioObjectManager : MonoBehaviour
{
	[Serializable]
	public struct AudioSources {
		public string name;
		public AudioSource source;
	}

	public AudioSources[] _Sources;

	public void PlaySource ( string name, int index = 1 ) {
		AudioSource Source = GetSource(name, index);
		if (Source != null) { Source.Play(); }
		else { Debug.LogError(name + index + " Is not valid"); }

	}

	public void StopSource ( string name, int index = -1 ) {
		AudioSource Source = GetSource(name, index);
		if (Source != null) { Source.Stop(); }
		else { Debug.LogError(name + index + " Is not valid"); }
	}

	//Gets the audio source object in the array. If a name is given find one that matches. If index is given, just use the source at that index in the array.
	private AudioSource GetSource ( string name, int index ) {
		if(_Sources == null || _Sources.Length == 0) { return null; }

		if (name != "")
		{
			foreach (AudioSources source in _Sources)
			{
				if (source.name == name)
					return source.source;
			}
		}

		if (_Sources.Length < index + 1 && index > -1)
			return _Sources[index].source;

		return null;

	}
}
