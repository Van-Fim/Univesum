using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class AICommandData
{
    public int spaceObjectId = 0;
    public int obstacleMask = 8;
    public float scanDistance = 1000f;
    public float raycastDistance = 300f;
    public float evasiveTurnSpeed = 100f;
    public float evasiveDuration = 1.5f; // Сколько времени уклоняться
    public bool isEvading = false;

    public Vector3 targetPosition;
    public Vector3 evadePosition;
    public float evadeTimer = 0f;
}
public class AICommand
{
    public Dictionary<string, float> mainParams = new Dictionary<string, float>();
    public Queue<AITask> taskQueue = new Queue<AITask>();
    public AITask currentTask;
    public string name;
    public string s_params;
    public static UnityAction<AIEvent> OnInterruptAction;

    public SpaceObject spaceObject;
    public bool isPlayerSpace;

    [Header("Collision Avoidance")]
    public LayerMask obstacleMask = 8;
    public float scanDistance = 1000f;
    public float raycastDistance = 300f;
    public float evasiveTurnSpeed = 100f;
    public float evasiveDuration = 1.5f; // Сколько времени уклоняться
    public bool isEvading = false;
    public AIEvadingEvent aIEvadingEvent;
    private Vector3 targetPosition;
    public Vector3 evadePosition;
    private float evadeTimer = 0f;
    public virtual AICommandData ReadData(AICommandData aICommandData)
    {
        if (aICommandData == null)
        {
            return null;
        }
        aICommandData.obstacleMask = this.obstacleMask;
        aICommandData.scanDistance = this.scanDistance;
        aICommandData.raycastDistance = this.raycastDistance;
        aICommandData.evasiveDuration = this.evasiveDuration;
        aICommandData.evasiveTurnSpeed = this.evasiveTurnSpeed;
        aICommandData.isEvading = this.isEvading;
        aICommandData.evadeTimer = this.evadeTimer;
        aICommandData.targetPosition = this.targetPosition;
        aICommandData.evadePosition = this.evadePosition;
        return aICommandData;
    }
    public virtual bool InstallData(AICommandData aICommandData)
    {
        bool ret = false;
        if (aICommandData == null)
        {
            return ret;
        }
        obstacleMask = aICommandData.obstacleMask;
        scanDistance = aICommandData.scanDistance;
        raycastDistance = aICommandData.raycastDistance;
        evasiveDuration = aICommandData.evasiveDuration;
        evasiveTurnSpeed = aICommandData.evasiveTurnSpeed;
        isEvading = aICommandData.isEvading;
        evadeTimer = aICommandData.evadeTimer;
        targetPosition = aICommandData.targetPosition;
        evadePosition = aICommandData.evadePosition;
        ret = true;
        return ret;
    }
    public virtual void Init()
    {
        OnInterruptAction += OnInterrupt;
    }
    public virtual void Destroy()
    {
        OnInterruptAction -= OnInterrupt;
    }
    public virtual void OnInterrupt(AIEvent interruptEvent)
    {
        if (spaceObject.id == interruptEvent.spaceObjectId)
        {

        }
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
