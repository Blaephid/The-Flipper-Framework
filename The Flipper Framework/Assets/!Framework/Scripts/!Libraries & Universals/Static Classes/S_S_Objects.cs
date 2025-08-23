using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_Objects
{
	//Takes a transform and returns returns a local transform that is equal in world space.
	//NOTE - This only works if the transform is the same rotation as its parent. If you want to rotate, only rotate children of this, as their scaling will be set back to a factor of Vector3.one
	public static Vector3 LockScale ( Transform transform, float lockTo = 0 ) {
		Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;

		Vector3 worldScale = transform.lossyScale;
		Vector3 localScale = transform.localScale;
		float averageScale = (Mathf.Abs(worldScale.x) + Mathf.Abs(worldScale.y) + Mathf.Abs(worldScale.z)) / 3;
		averageScale = lockTo > 0 ? lockTo : averageScale;

		//Applies inverse scale to parents world scale, esentially resetting scale to one for its children. This only works if the object has no local rotation.
		Vector3 newLocalScale = new Vector3(averageScale  / parentScale.x
			,averageScale / parentScale.y
			,averageScale / parentScale.z);
		return newLocalScale;
	}


	//Takes an animator and the name of a trigger, then after x frames, sends that trigger to than animator.
	public static IEnumerator TriggerAnimatorAfterDelay ( Animator Animator, string trigger, int frames = 0 ) {
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		Animator.SetTrigger(trigger);
	}

	//Sames as above but with a specific animation component rather than an animator.
	public static IEnumerator TriggerAnimationAfterDelay ( Animation Clip, int frames ) {
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		Clip.Play();
	}


	//Takes an object and finds the scale needed for its bounds to fit neatly with the camera field of view, so it fills the camera edges
	public static Vector2 GetScaleToFitCameraBounds(Camera Cam, float zOffset, Transform transform, bool setTo ) {

		float height = 2f * zOffset * Mathf.Tan(Cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float width = height * Cam.aspect;

		if(setTo)
		{
			transform.parent = Cam.transform;
			transform.localPosition = new Vector3(0, 0, zOffset);
			transform.localScale = new Vector3(width, height, 1f);
		}

		return new Vector2(width, height);
	}

	//Used for fading audio in or out.
	public static IEnumerator LerpAudioSourceVolume(AudioSource Source, float duration, float targetVolume ) {
		if(!Source) { yield break; }
		float initialVolume = Source.volume;
		float time = 0;

		float lerpProgress = 0;

		while (Source.volume != targetVolume)
		{
			yield return null;
			time += Time.deltaTime;
			lerpProgress = time / duration;
			Source.volume = Mathf.Lerp(initialVolume, targetVolume, lerpProgress);
		}
	}
}
