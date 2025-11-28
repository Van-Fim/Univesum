using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Loadout
{
    public string name;
    public List<LoadoutHP> hardpoints;
}
[System.Serializable]
public class LoadoutHP
{
    public string hardpoint;
    public string item;
}