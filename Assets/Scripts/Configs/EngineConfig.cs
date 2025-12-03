using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class EngineConfig
{
    public int maxSpeed = 700;
    public int rotationSpeed = 150;
    public int accelerationSpeed = 100;

    public string soundIdle;

    public Color32 color01;
    public Color32 color02;

    public List<JetEngineController> jetEngineControllers = new List<JetEngineController>();
}