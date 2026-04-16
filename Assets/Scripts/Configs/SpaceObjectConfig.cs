using UnityEngine;
using Zenject;
[System.Serializable]
public class SpaceObjectConfig
{
    public string name;
    public string pathToModel;
    public string chinldName;
    public string pathToHardpoints;
    public string pathToMaterial;
    public float scale = 1f;
    public float mass;
    public float linearDrag;
    public float angularDrag;
}