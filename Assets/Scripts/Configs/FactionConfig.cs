using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
[Serializable]
public class FactionConfig
{
    public string name;
    public Color32 color;
    public List<FactionRelationshipConfig> relationships;
    public int systems_count_min;
    public int systems_count_max;
    public int systems_count;
    public bool is_started;
}
