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
public class AIFollowFightEvent: AIEvent
{

}
public class AIEvadingEvent: AIEvent
{
    public Vector3 evadingPosition;
    public Vector3 evadingDirection;
    public float evadingDuration;
    public float evadingSpeed;
    public float evadingTime = 4f;
    public int targetScale;
}
