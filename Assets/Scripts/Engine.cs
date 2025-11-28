using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class Engine
{
    public int max_speed = 700;
    public int rotation_speed = 150;
    public int acceleration_speed = 100;

    public Color32 color01;
    public Color32 color02;

    public List<JetEngineController> jetEngineControllers = new List<JetEngineController>();
}