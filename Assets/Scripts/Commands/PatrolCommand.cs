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
        if (currentTask == null)
        {
            targetPosition = UnityEngine.Random.insideUnitSphere * mainParams["range"];
            taskQueue.Enqueue(new MoveToTask(this, targetPosition));
            currentTask = taskQueue.Dequeue();
        }

        base.Execute();
    }
    public override void OnInterrupt(AIEvent interruptEvent)
    {
        base.OnInterrupt(interruptEvent);
        if (spaceObject.id == interruptEvent.spaceObjectId && interruptEvent is AIEnemyDetectedEvent && spaceObject.rigidbody != null)
        {
            taskQueue.Clear();
            taskQueue.Enqueue(new MoveAttack(this, interruptEvent.spaceObjectId, interruptEvent.targetId, spaceObject.GetFiringRange()));
            currentTask = taskQueue.Dequeue();
        }
        if (spaceObject.id == interruptEvent.spaceObjectId && interruptEvent is AIFollowFightEvent && spaceObject.rigidbody != null)
        {
            taskQueue.Clear();
            taskQueue.Enqueue(new FollowFight(this, interruptEvent.spaceObjectId, interruptEvent.targetId, spaceObject.GetFiringRange()));
            currentTask = taskQueue.Dequeue();
        }
        if (spaceObject.id == interruptEvent.spaceObjectId && interruptEvent is AIEvadingEvent && spaceObject.rigidbody != null)
        {
            isEvading = true;
        }
    }
    public override void CheckForInterrupts()
    {
        StarSystem sys = spaceObject.GetStarSystem();
        for (int i = 0; i < sys.allObjs.Count; i++)
        {
            SpaceObject so = sys.allObjs[i];
            if (so == spaceObject)
            {
                continue;
            }
            float distance = Vector3.Distance(so.transform.position, spaceObject.transform.position);
            Faction fc = so.GetOwner();
            float relation = fc.GetRelation(spaceObject.GetOwner().name);
            if (currentTask is MoveToTask && distance < mainParams["range"] && relation < -5000)
            {
                currentTask.Finish();
                currentTask = null;

                AIEnemyDetectedEvent enemyDetected = new AIEnemyDetectedEvent() { spaceObjectId = spaceObject.id, targetId = so.id };
                AICommand.InvokeInterrupt(enemyDetected);

                return;
            }
        }
    }
    public override bool IsCompleted => false;
}
