using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Cost", order = 1)]
public class Cost : ScriptableObject
{
    public List<int> costAmounts = new List<int>();
    public List<int> costTypes = new List<int>();
}
