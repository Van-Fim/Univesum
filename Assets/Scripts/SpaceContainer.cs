using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class SpaceContainer : MonoBehaviour
{
    public static SpaceContainer singleton;
    public void Start()
    {
        singleton = this;
    }
}