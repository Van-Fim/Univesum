using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

public interface IAITask
{
    bool Execute(SpaceObject spaceObject);
    bool IsFinished { get; }
    public AICommand AICommand { get; set; }
    void Finish();
    public void Evading();
}

// Пример конкретной задачи: Полет к точке
public class AITask
{
    private Vector3 targetPosition;
    public Vector3 evadePosition;
    public bool IsFinished { get; set; }
    public bool IsEvadePositionChanged = false;
    public AICommand AICommand { get; set; }

    private int spaceObjectId;
    private int targetObjectId;
    public SpaceObject spaceObject;
    public SpaceObject targetObject;

    public AITask()
    {

    }

    public virtual bool Execute(SpaceObject spaceObject)
    {
        // Логика перемещения корабля к targetPosition
        // Если достигли точки -> IsFinished = true

        return true;
    }

    public virtual void Evading(AIEvadingEvent ev)
    {
        if (ev.spaceObjectId != spaceObject.id) return;
        if (!IsEvadePositionChanged)
        {
            evadePosition = AICommand.evadePosition;
            IsEvadePositionChanged = true;
        }
        float distanceToPoint = Vector3.Distance(spaceObject.transform.position, evadePosition);
        if (distanceToPoint < 10f)
        {
            AICommand.isEvading = false;
            IsEvadePositionChanged = false;
        }

        spaceObject.spaceObjectController.Turn(evadePosition);
        spaceObject.spaceObjectController.Move(evadePosition);

        if (AICommand.isEvading)
        {
            ev.evadingTime -= Time.deltaTime;
            if (ev.evadingTime <= 0)
            {
                //AICommand.isEvading = false;
                //ev.evadingTime = 0;
            }
        }

        AICommand.evadePosition = evadePosition;
    }

    public virtual void Finish()
    {
        IsFinished = true;
    }
}
