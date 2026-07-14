using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

public interface IAITask
{
    bool Execute(Ship ship, params object[] parameters);
    bool IsFinished { get; }
}

// Пример конкретной задачи: Полет к точке
public class IdleTask : IAITask
{
    private Vector3 targetPosition;
    public bool IsFinished { get; private set; }

    public IdleTask()
    {}

    public bool Execute(Ship ship, params object[] parameters)
    {
        // Логика перемещения корабля к targetPosition
        // Если достигли точки -> IsFinished = true
        return true;
    }
}
