using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
// Пример конкретной задачи: Полет к точке
public class MoveAttack : IAITask
{
    private int spaceObjectId;
    private int targetObjectId;
    SpaceObject spaceObject;
    SpaceObject targetObject;
    public bool IsFinished { get; private set; }

    public MoveAttack()
    { }

    public MoveAttack(int spaceObjectId, int targetObjectId)
    {
        Universe universe = Universe.singleton;
        spaceObject = universe.allSpaceObjects.Find(x => x.id == spaceObjectId);
        targetObject = universe.allSpaceObjects.Find(x => x.id == targetObjectId);
    }

    public bool Execute(SpaceObject spaceObject)
    {
        // Логика перемещения корабля к targetPosition
        // Если достигли точки -> IsFinished = true

        spaceObject.spaceObjectController.Turn(targetObject.transform.position);
        spaceObject.spaceObjectController.Move(1, targetObject.transform);
        return true;
    }

    public void Finish()
    {
        IsFinished = true;
    }
}
