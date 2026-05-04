using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class SpaceConfigSkybox
{
    public string name;
    public Color32 color;
}
[System.Serializable]
public class SpaceConfigListItem
{
    public List<string> skyboxNames;
    public List<SpaceAsteroidFieldListItem> asteroidFields;
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
    public string faction;
    public string name;
    public int rangeMin;
    public int rangeMax;
    public int YMin;
    public int YMax;
    public List<SpaceAsteroidFieldListItem> asteroidFields;
    public List<SpaceConfigListItem> list;
    public List<SpaceConfigSkybox> skyboxes;
}
[System.Serializable]
public class SpaceAsteroidFieldListItem
{
    public Vector3 shapeSize;
    public string name;
    public int rangeMin;
    public int rangeMax;
    public int countMin;
    public int countMax;
    public int YMin;
    public int YMax;
}