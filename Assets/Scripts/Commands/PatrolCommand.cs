using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class PatrolCommand : AICommand
{
    public override void UpdateCommand()
    {
        base.UpdateCommand();
    }
    public override void Execute()
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
    public override bool IsCompleted => false;
}
