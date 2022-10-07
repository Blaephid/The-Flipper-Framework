using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterResources : MonoBehaviour
{
    [Header("Key Objects")]
    public GameObject JumpBall;
    public GameObject characterCapsule;
    public GameObject crouchCapsule;

    [Header("Model / Skin")]
    public Animator CharacterAnimator;
    public SonicSoundsControl SoundControl;
    public SonicEffectsControl EffectsControl;
}
