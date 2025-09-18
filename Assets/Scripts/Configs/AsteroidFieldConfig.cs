using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AsteroidFieldConfig
{
    public string name;
    public List<AsteroidFieldItemConfig> asteroids;
}
[System.Serializable]
public class AsteroidFieldItemConfig
{
    public string name;
    public string spaceObjectPath;
    public int poolSize = 20;
    public float scaleMin = 500f;
    public float scaleMax = 3000f;
}