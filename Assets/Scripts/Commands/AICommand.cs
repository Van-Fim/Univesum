using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Events;

public class AICommand
{
    public Dictionary<string, float> mainParams = new Dictionary<string, float>();
    public Queue<IAITask> taskQueue = new Queue<IAITask>();
    public IAITask currentTask;

    public static UnityAction<AIEvent> OnInterruptAction;

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
    public virtual void Init()
    {
        OnInterruptAction += OnInterrupt;
    }
    public virtual void OnInterrupt(AIEvent interruptEvent)
    {

    }
    public static void InvokeInterrupt(AIEvent interruptEvent)
    {
        OnInterruptAction?.Invoke(interruptEvent);
    }
    public virtual void UpdateCommand()
    {
        CheckSpace();
        CheckForInterrupts();
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
    public virtual void CheckForInterrupts()
    {

    }
    public virtual bool IsCompleted => false;
}
