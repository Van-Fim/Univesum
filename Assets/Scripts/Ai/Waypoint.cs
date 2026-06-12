using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
public class Waypoint
{
    public Vector3 position;
    public SpAIExecutor spAIExecutor;
    public float creationTime;
    public int waypointID;

    // Визуальные настройки
    public Color gizmoColor = Color.green;
    public float gizmoRadius = 50f;

    public Waypoint(Vector3 position, int id)
    {
        this.position = position;
        this.waypointID = id;
    }

    public void Destroy()
    {
        if (spAIExecutor != null && spAIExecutor.waypoints.Contains(this))
        {
            spAIExecutor.waypoints.Remove(this);
        }
    }
}
