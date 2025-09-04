using UnityEngine;

public class Player
{
    private PlayerController currentController;

    public bool IsInShip { get; private set; }

    private void Awake()
    {
        SwitchToShip();
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

    public PlayerController GetCurrentController()
    {
        return currentController;
    }
}
