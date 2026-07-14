using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
public static class DirectionHelper
{
    // Кэшируем вектора для избежания аллокаций
    private static Vector3 cachedDirection;
    private static Vector3 cachedLocalDirection;

    public static bool IsInFront(Transform from, Transform to, float maxAngle = 90f)
    {
        if (from == null || to == null) return false;

        cachedDirection = (to.position - from.position).normalized;
        float dot = Vector3.Dot(from.forward, cachedDirection);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        return angle < maxAngle;
    }

    public static bool IsInFront(Transform from, Vector3 position, float maxAngle = 90f)
    {
        if (from == null) return false;

        cachedDirection = (position - from.position).normalized;
        float dot = Vector3.Dot(from.forward, cachedDirection);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        return angle < maxAngle;
    }

    public static float GetAngleToTarget(Transform from, Transform to)
    {
        if (from == null || to == null) return 180f;

        cachedDirection = (to.position - from.position).normalized;
        float dot = Vector3.Dot(from.forward, cachedDirection);
        return Mathf.Acos(dot) * Mathf.Rad2Deg;
    }

    public static Vector3 GetLocalDirection(Transform from, Transform to)
    {
        if (from == null || to == null) return Vector3.zero;

        cachedDirection = (to.position - from.position).normalized;
        return from.InverseTransformDirection(cachedDirection);
    }
}
