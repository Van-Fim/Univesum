using UnityEngine;
using Zenject;
public class Player
{
    public SpaceObjectController currentController;
    private readonly SignalBus _signalBus;
    private readonly Universe _universe;

    public bool IsInShip { get; private set; }
    public Player(
            Universe universe,
            SignalBus signalBus)
    {
        _universe = universe;
        _signalBus = signalBus;
    }

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
