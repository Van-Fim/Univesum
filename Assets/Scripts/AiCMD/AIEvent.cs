using UnityEngine;

public class AIEvent
{
    public int spaceObjectId;
    public int targetId;
}
public class AIEnemyDetectedEvent : AIEvent
{

}
public class AITargetLostEvent: AIEvent
{

}
