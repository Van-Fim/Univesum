using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

public interface IAITask
{
    bool Execute(SpaceObject spaceObject);
    bool IsFinished { get; }
    public AICommand AICommand{ get; set; }
    void Finish();
    public void Evading();
}

// Пример конкретной задачи: Полет к точке
public class AITask : IAITask
{
    private Vector3 targetPosition;
    public bool IsFinished { get; set; }
    public AICommand AICommand{ get; set; }
    public AITask()
    {}

    public bool Execute(SpaceObject spaceObject)
    {
        // Логика перемещения корабля к targetPosition
        // Если достигли точки -> IsFinished = true

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
