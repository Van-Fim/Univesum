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
            taskQueue.Enqueue(new MoveToTask(targetPosition));
            currentTask = taskQueue.Dequeue();
        }

        base.Execute();
    }
    public override bool CheckForInterrupts()
    {
        bool result = false;
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
            if (distance < mainParams["range"] && relation < -5000)
            {
                currentTask.Finish();
                currentTask = null;
                result = false; // Если поставлю true, то выполнение задач остановлено

                taskQueue.Enqueue(new MoveAttack(so));
                currentTask = taskQueue.Dequeue();

                return result;
            }
        }
        return result;
    }
    public override bool IsCompleted => false;
}
