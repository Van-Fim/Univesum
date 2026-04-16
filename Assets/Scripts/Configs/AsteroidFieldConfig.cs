using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AsteroidFieldConfig
{
    public string name;
    public List<AsteroidFieldItemConfig> asteroids;
    public List<AsteroidSpeedThresholdsConfig> speedThresholds;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 shapeSize;
    public int countMin;
    public int countMax;
    public bool is_infinity;
    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
[System.Serializable]
public class AsteroidFieldItemConfig
{
    public string name;
    public string spaceObjectPath;
    public int poolSize = 20;
    public float scaleMin = 10f;
    public float scaleMax = 10f;
}
[System.Serializable]
public class AsteroidSpeedThresholdsConfig
{
    public int speed = 20;
    public int scale = 20;
}