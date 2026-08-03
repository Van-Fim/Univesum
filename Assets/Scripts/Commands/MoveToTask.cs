using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
// Пример конкретной задачи: Полет к точке
public class MoveToTask : IAITask
{
    private Vector3 targetPosition;
    public bool IsFinished { get; set; }
    public AICommand AICommand{ get; set; }

    public MoveToTask()
    { }

    public MoveToTask(AICommand command, Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        AICommand = command;
    }

    public bool Execute(SpaceObject spaceObject)
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
            Evading();
            return true;
        }

        return true;
    }

    public void Evading()
    {

    }

    public void Finish()
    {
        IsFinished = true;
    }
}
