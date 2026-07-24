using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
// Пример конкретной задачи: Полет к точке
public class MoveAttack : IAITask
{
    private SpaceObject spaceObject;
    public bool IsFinished { get; private set; }

    public MoveAttack()
    { }

    public MoveAttack(SpaceObject spaceObject)
    {
        this.spaceObject = spaceObject;
    }

    public bool Execute(SpaceObject spaceObject)
    {
        // Логика перемещения корабля к targetPosition
        // Если достигли точки -> IsFinished = true

        return true;
    }

    public void Finish()
    {
        IsFinished = true;
    }
}
