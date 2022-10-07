using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Voices", menuName = "SonicGT/Character/New Voices")]
public class CharacterGameVoice : ScriptableObject
{

    public AudioClip[] CombatVoiceClips;
    public AudioClip[] EnemyCombatVoiceClips;
    public AudioClip[] JumpingVoiceClips;
    public AudioClip[] PainVoiceClips;
    public AudioClip[] Taunts;
    public AudioClip[] Idle;
}
