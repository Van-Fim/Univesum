using UnityEngine;

public class SpaceObjectOnTakeDamage
{
    public int value;
    public SpaceObject target;
    public SpaceObject attacker;

    public SpaceObjectOnTakeDamage(SpaceObject attacker, SpaceObject target, int value)
    {
        this.attacker = attacker;
        this.target = target;
        this.value = value;
    }
}
