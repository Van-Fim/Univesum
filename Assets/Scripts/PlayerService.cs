using UnityEngine;
using Zenject;

public class PlayerService
{
    private readonly Player _player;
    private readonly SignalBus _signalBus;

    [Inject]
    public PlayerService(Player player, SignalBus signalBus)
    {
        _player = player;
        _signalBus = signalBus;
    }
}
