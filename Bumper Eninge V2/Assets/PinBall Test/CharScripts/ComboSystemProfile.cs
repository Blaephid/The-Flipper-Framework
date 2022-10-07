using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Combo Profile", menuName = "SonicGT/General/Combo Profile")]
public class ComboSystemProfile : ScriptableObject
{
    [Header("Color Scheme")]
    [Space]
    public Color Phase2;
    public Color Phase3;

    [Header("Naming System")]
    [Space]
    public string SpeedPh1;
    public string SpeedPh2, SpeedPh3;
    [Space]
    public string RailPh1;
    public string RailPh2, RailPh3;
    [Space]
    public string TrickPh1;    
    public string TrickPh2, TrickPh3;
    [Space]
    public string EnemyPh1;
    public string EnemyPh2, EnemyPh3;
    [Space]
    public string RingPh1;
    public string RingPh2, RingPh3;
}
