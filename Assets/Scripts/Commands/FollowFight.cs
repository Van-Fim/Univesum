using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
// Пример конкретной задачи: Полет к точке
public class FollowFight : AITask
{
    public float firingRange = 560;

    public FollowFight()
    { }

    public FollowFight(AICommand command, int spaceObjectId, int targetObjectId, float firingRange)
    {
        Universe universe = Universe.singleton;
        spaceObject = universe.allSpaceObjects.Find(x => x.id == spaceObjectId);
        targetObject = universe.allSpaceObjects.Find(x => x.id == targetObjectId);
        this.firingRange = firingRange;
        AICommand = command;
    }

    public override bool Execute(SpaceObject spaceObject)
    {
        if (!AICommand.isEvading)
        {
            //...................................
            // СРАЖЕНИЕ, СБЛИЖЕНИЕ, УВОРОТ, ТРЮКИ
            // ..................................
            spaceObject.spaceObjectController.Turn(targetObject.transform.position);
            spaceObject.spaceObjectController.Move(1, targetObject.transform);
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
