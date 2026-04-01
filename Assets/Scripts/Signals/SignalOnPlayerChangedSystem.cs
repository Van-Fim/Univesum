using Unity.VisualScripting;
using UnityEngine;
public class SignalOnPlayerChangedSystem
{
    public SpaceObject spaceObject;
    public StarSystem starSystem;
    public SignalOnPlayerChangedSystem(SpaceObject spaceObject, StarSystem starSystem)
    {
        this.spaceObject = spaceObject;
        this.starSystem = starSystem;
    }
}
