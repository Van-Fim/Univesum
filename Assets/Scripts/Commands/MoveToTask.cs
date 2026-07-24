using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
// Пример конкретной задачи: Полет к точке
public class MoveToTask : IAITask
{
    private Vector3 targetPosition;
    public bool IsFinished { get; private set; }

    public MoveToTask()
    { }

    public MoveToTask(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public bool Execute(SpaceObject spaceObject)
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
        return true;
    }

    public void Finish()
    {
        IsFinished = true;
    }
}
