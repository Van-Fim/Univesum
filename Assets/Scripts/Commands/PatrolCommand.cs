using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class PatrolCommand : AICommand
{
    Vector3 targetPosition;

    public override void UpdateCommand()
    {
        base.UpdateCommand();
    }
    public override void Execute()
    {
        if (currentTask is IdleTask)
        {
            targetPosition = UnityEngine.Random.insideUnitSphere * mainParams["range"];
            taskQueue.Enqueue(new MoveToTask(targetPosition));
            currentTask = taskQueue.Dequeue();
        }
        Debug.Log($"Executing: {currentTask?.GetType().Name ?? "IdleTask"}");
        base.Execute();
    }
    public override bool CheckForInterrupts()
    {
        return false;
    }
    public override bool IsCompleted => false;
}
