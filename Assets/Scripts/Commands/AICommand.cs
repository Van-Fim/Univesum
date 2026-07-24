using System.Collections.Generic;
using UnityEngine;

public class AICommand
{
    public Dictionary<string, float> mainParams = new Dictionary<string, float>();
    public Queue<IAITask> taskQueue = new Queue<IAITask>();
    public IAITask currentTask;

    public SpaceObject spaceObject;
    public bool isPlayerSpace;

    [Header("Collision Avoidance")]
    public LayerMask obstacleMask = 8;
    public float scanDistance = 1000f;
    public float raycastDistance = 300f;
    public float evasiveTurnSpeed = 100f;
    public float evasiveDuration = 1.5f; // Сколько времени уклоняться
    private bool isEvading = false;
    private Vector3 evadeDirection;
    private float evadeTimer = 0f;
    public virtual void UpdateCommand()
    {
        CheckSpace();
        if (CheckForInterrupts()) return;
        Execute();
    }
    public virtual void Execute()
    {
        if (currentTask == null && taskQueue.Count > 0)
        {
            currentTask = taskQueue.Dequeue();
        }
        if (currentTask != null)
        {
            if (currentTask != null && !currentTask.IsFinished)
            {
                currentTask.Execute(spaceObject);
            }
            if (currentTask.IsFinished)
            {
                currentTask = null;
            }
        }
    }
    public virtual void CheckSpace()
    {
        isPlayerSpace = spaceObject.GetStarSystem() == PlayerService.singleton.GetStarSystem();
    }
    public virtual bool CheckForInterrupts()
    {
        return false;
    }
    public virtual bool IsCompleted => false;
}
