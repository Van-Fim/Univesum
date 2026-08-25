using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
// Пример конкретной задачи: Полет к точке
public class MoveAttack : AITask
{
    private int spaceObjectId;
    private int targetObjectId;
    public float firingRange = 560;
    SpaceObject enemy;

    public MoveAttack()
    { }

    public MoveAttack(AICommand command, int spaceObjectId, int targetObjectId, float firingRange)
    {
        Universe universe = Universe.singleton;
        spaceObject = universe.allSpaceObjects.Find(x => x.id == spaceObjectId);
        TargetObject = universe.allSpaceObjects.Find(x => x.id == targetObjectId);
        this.firingRange = firingRange;
        AICommand = command;
    }

    public override bool Execute(SpaceObject spaceObject)
    {
        if (!AICommand.isEvading)
        {
            // Логика перемещения корабля к targetPosition
            // Если достигли точки -> IsFinished = true
            float distance = Vector3.Distance(spaceObject.transform.position, TargetObject.transform.position);
            StarSystem starSystem = spaceObject.GetStarSystem();
            StarSystem tstarSystem = TargetObject.GetStarSystem();
            if (distance <= firingRange && starSystem == tstarSystem)
            {
                if (enemy == null)
                {
                    AIFollowFightEvent followAttack = new AIFollowFightEvent() { spaceObjectId = spaceObject.id, targetId = TargetObject.id };
                    AICommand.InvokeInterrupt(followAttack);
                    enemy = TargetObject;
                }
            }
            else if (enemy == TargetObject)
            {
                enemy = null;
            }

            spaceObject.spaceObjectController.Turn(TargetObject.transform.position);
            spaceObject.spaceObjectController.Move(1, TargetObject.transform);
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
