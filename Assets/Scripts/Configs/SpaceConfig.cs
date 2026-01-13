using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class SpaceConfigListItem
{
    public List<string> skyboxNames;
    public string name;
    public int rangeMin;
    public int rangeMax;
    public int countMin;
    public int countMax;
    public int YMin;
    public int YMax;
}
[System.Serializable]
public class SpaceConfig
{
    public int safeRange = 10;
    public string name;
    public int rangeMin;
    public int rangeMax;
    public int YMin;
    public int YMax;
    public List<SpaceConfigListItem> list;
}