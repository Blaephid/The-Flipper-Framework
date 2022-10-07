using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Abilities
{
    public MonoBehaviour abilityScript;

    public bool FlightAbility;
    public bool SpeedAbility;
    public bool PowerAbility;
    public bool OtherAbility;

    public int ChunkCost = 1;
    public int RingCost = 0;

    public int level;
}
