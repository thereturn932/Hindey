using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Building")]
[Serializable]
public class Building : ScriptableObject
{
    public string buildingName;
    public int buildingCost;
    public int[] buildingModifier = new int[8];
    public int maxBuilding;
}
