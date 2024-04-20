using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class S_PlayerEvents : MonoBehaviour
{
	public UnityEvent                       _OnGrounded;        //Event called when isGrounded is set to true from false, remember to assign what methods to call in the editor.
	public UnityEvent                       _OnLoseGround;        //Event called when isGrounded is set to false from true.
	public UnityEvent<Collider>             _OnTriggerEnter;        //Event called when entering a trigger through the built in method.
	public UnityEvent<Collider>             _OnTriggerExit;        //Event called when exitting a trigger through the built in method.
	public UnityEvent<Collision>		_OnCollisionEnter;        //Event called when start collision through the built in method.
	public UnityEvent<Collider>		_OnTriggerStay;        //Event called each frame when in a trigger.
	public UnityEvent                       _OnTriggerAirLauncher;        //Event called each frame when in a trigger.
}
