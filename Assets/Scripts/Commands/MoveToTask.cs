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

    public bool Execute(Ship ship, params object[] parameters)
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
