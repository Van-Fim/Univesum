using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class PowerGeneratorConfig
{
    public int maxEnergy = 700;
    public int regenRate = 1;
    public int regenStepValue = 10;
    public float startRegenDelay = 1f;
    public float delayPenalty = 2f;
}
