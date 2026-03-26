using UnityEngine;

public class SpaceObjectOnDestroyHide
{
    public SpaceObject target;
    public SpaceObject attacker;

    public SpaceObjectOnDestroyHide(SpaceObject attacker, SpaceObject target)
    {
        this.attacker = attacker;
        this.target = target;

    }
}
