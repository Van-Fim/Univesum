using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
// Пример конкретной задачи: Полет к точке
public class FollowFight : IAITask
{
    private int spaceObjectId;
    private int targetObjectId;
    public float firingRange = 560;
    SpaceObject spaceObject;
    SpaceObject targetObject;
    public bool IsFinished { get; set; }
    public AICommand AICommand{ get; set; }

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

    public bool Execute(SpaceObject spaceObject)
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
