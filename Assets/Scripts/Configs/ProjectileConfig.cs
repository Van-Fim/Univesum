using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileConfig
{
    public float damage;
    public float speed;
    public float lifetime;
    public string pathToModel;
    public Color32 baseColor;

    [Header("Tail (LineRenderer)")]
    public bool tailEnabled = false;
    public float tailMaxLength = 10f;
    public float tailMinPointDistance = 0.25f;
    public int tailMaxPoints = 128;

    // Optional visual settings (in case you want to drive them from config)
    public float tailStartWidth = 0.1f;
    public float tailEndWidth = 0.0f;
}
