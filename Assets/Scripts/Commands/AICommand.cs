using System.Collections.Generic;
using UnityEngine;

public class AICommand
{
    public Dictionary<string, float> mainParams = new Dictionary<string, float>();
    public Queue<IAITask> taskQueue = new Queue<IAITask>();
    public IAITask currentTask;

    public SpaceObject spaceObject;
    public bool isPlayerSpace;
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
