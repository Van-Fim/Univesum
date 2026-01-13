using UnityEngine;

public class SpaceObjectOnDestroy
{
    public SpaceObject target;
    public SpaceObject attacker;

    public SpaceObjectOnDestroy(SpaceObject attacker, SpaceObject target)
    {
        this.attacker = attacker;
        this.target = target;

    }
}
