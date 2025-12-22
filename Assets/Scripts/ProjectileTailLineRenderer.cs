using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tail for a projectile based on LineRenderer with a hard cap on world-space length.
/// Designed to work with pooling: call ResetTail() on spawn/launch and despawn.
/// </summary>
[DisallowMultipleComponent]
public class ProjectileTailLineRenderer : MonoBehaviour
{
    [Header("References")]
    public LineRenderer line;

    [Header("Tail params")]
    [Min(0f)] public float maxLength = 10f;           // meters in world space
    [Min(0.001f)] public float minPointDistance = 0.25f;
    [Min(2)] public int maxPoints = 128;

    private readonly List<Vector3> _points = new(128);

    private void Reset()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Awake()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line != null)
        {
            line.useWorldSpace = true;
            line.positionCount = 0;
            line.material = Resources.Load<Material>("Materials/ProjectileTrail");
        }
    }

    public void ApplyConfig(ProjectileConfig config)
    {
        if (config == null) return;

        if (!config.tailEnabled)
        {
            if (line != null) line.enabled = false;
            return;
        }

        if (line != null)
        {
            line.enabled = true;
            line.material = Resources.Load<Material>("Materials/ProjectileTrail");
        }
        maxLength = Mathf.Max(0f, config.tailMaxLength);
        minPointDistance = Mathf.Max(0.001f, config.tailMinPointDistance);
        maxPoints = Mathf.Max(2, config.tailMaxPoints);

        // Optional visual settings
        if (line != null)
        {
            line.startWidth = config.tailStartWidth;
            line.endWidth = config.tailEndWidth;
        }
    }

    public void ResetTail()
    {
        _points.Clear();
        if (line != null)
            line.positionCount = 0;
    }

    private void LateUpdate()
    {
        if (line == null || !line.enabled) return;

        Vector3 p = transform.position;

        if (_points.Count == 0)
        {
            _points.Add(p);
            line.positionCount = 1;
            line.SetPosition(0, p);
            return;
        }

        // Add a new point only if we moved enough to avoid too many points at high FPS.
        if (Vector3.Distance(_points[^1], p) >= minPointDistance)
        {
            _points.Add(p);

            // Hard cap point count (drop oldest)
            if (_points.Count > maxPoints)
                _points.RemoveAt(0);

            TrimToMaxLength();
            SyncRenderer();
        }
        else
        {
            // Always update head position to avoid a "stuck" head.
            _points[^1] = p;
            TrimToMaxLength();
            SyncRenderer();
        }
    }

    private void TrimToMaxLength()
    {
        if (maxLength <= 0f)
        {
            // keep only head
            if (_points.Count > 1)
                _points.RemoveRange(0, _points.Count - 1);
            return;
        }

        // Ensure polyline length from head backwards is <= maxLength.
        // We walk from the newest point (head) towards the oldest (tail).
        float remaining = maxLength;

        for (int i = _points.Count - 1; i > 0; i--)
        {
            float seg = Vector3.Distance(_points[i], _points[i - 1]);
            if (seg <= remaining)
            {
                remaining -= seg;
                continue;
            }

            // Need to cut inside this segment.
            if (seg > 0.0001f)
            {
                Vector3 a = _points[i];       // newer
                Vector3 b = _points[i - 1];   // older

                float t = remaining / seg; // 0..1
                Vector3 cutPoint = Vector3.Lerp(a, b, t);

                _points[i - 1] = cutPoint;
            }

            // Remove everything older than (i-1)
            if (i - 1 > 0)
                _points.RemoveRange(0, i - 1);

            break;
        }
    }

    private void SyncRenderer()
    {
        int count = _points.Count;
        line.positionCount = count;
        for (int i = 0; i < count; i++)
            line.SetPosition(i, _points[i]);
    }
}
