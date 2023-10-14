using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Combo Profile", menuName = "SonicGT/General/Combo Global Profile")]
public class ComboSystemProfileGeneral : ScriptableObject
{
    [System.Serializable]
    public class ComboRelease
    {
        public string Name;
        public int Threshold;
        public Color EffectColor;
    }


    [Header("Conditions")]
    public float TimeOut = 0.5f;
    public float playerMinSpeed = 19.5f;
    public float secondStarDivider = 3;
    public float CapOff()
    {
        return (3 + (3 * secondStarDivider));
    }
    [Header("Progression")]
    [Tooltip("playerspeed - minSpeedCondition * Multiplier , 1 equals 1 star. final value would be points per second. so mult .01 to 100 speed will give 1 point/s")]
    public float SpeedRewardMultiplier = 0.005f;
    [Tooltip("Point Per Seconds * playerSpeed")]
    public float RailtimeRewardMultiplier = 0.01f;
    public float TrickAdition = 0.5f;
    public float EnemyAdition = 0.25f;
    public float RingAdition = 0.05f;


    [Header("Combo Releases")]
    public ComboRelease[] ComboReleases;
}
