using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
// Пример конкретной задачи: Полет к точке
public class MoveToTask : AITask
{
    private Vector3 targetPosition;

    public MoveToTask()
    { }

    public MoveToTask(AICommand command, Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        AICommand = command;
    }

    public override bool Execute(SpaceObject spaceObject)
    {
        if (!AICommand.isEvading)
        {
            // Логика перемещения корабля к targetPosition
            // Если достигли точки -> IsFinished = true
            Vector3 wpPosition = targetPosition;
            float distanceToWaypoint = Vector3.Distance(spaceObject.transform.position, wpPosition);
            if (PlayerService.singleton.GetStarSystem() == spaceObject._StarSystem)
            {
                //Debug.Log($"Distance: {distanceToWaypoint} = {wpPosition}");
            }
            if (distanceToWaypoint < 300f)
            {
                IsFinished = true;
                return true;
            }
            if (PlayerService.singleton.GetStarSystem() == spaceObject._StarSystem)
            {
                wpPosition = SpaceContainer.singleton.transform.position + targetPosition;
            }
            spaceObject.spaceObjectController.Turn(wpPosition);
            spaceObject.spaceObjectController.Move(wpPosition);
        }
        else
        {
            Evading(AICommand.aIEvadingEvent);
            return true;
        }

        return true;
    }

    public override void Finish()
    {
        IsFinished = true;
    }
}
