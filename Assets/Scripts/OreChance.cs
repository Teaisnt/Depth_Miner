using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/OreChance", order = 1)]
public class OreChance : ScriptableObject
{
    public List<int> oreChances = new List<int>();
}
