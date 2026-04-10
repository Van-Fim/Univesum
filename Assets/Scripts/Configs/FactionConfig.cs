using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
[Serializable]
public class FactionConfig
{
    public string name;
    public List<FactionRelationshipConfig> relationship;
}