using UnityEngine;
using Zenject;
public class Player
{
    public SpaceObjectController currentController;
    public static Player singleton;

    public bool IsInShip { get; private set; }

    public bool SwitchToShip()
    {
        IsInShip = true;

        Debug.Log("Switched to Ship Controller");
        return true;
    }

    public bool SwitchToSpacesuit()
    {
        IsInShip = false;

        Debug.Log("Switched to Spacesuit Controller");
        return true;
    }

    public SpaceObjectController GetCurrentController()
    {
        return currentController;
    }
}
