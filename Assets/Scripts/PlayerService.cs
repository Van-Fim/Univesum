using UnityEngine;
using Zenject;

public class PlayerService
{
    public Player _player;
    private readonly SignalBus _signalBus;
    private readonly Universe _universe;

    private StarSystem _starSystem;

    [Inject]
    public PlayerService(Player player, SignalBus signalBus, Universe universe)
    {
        _player = player;
        _signalBus = signalBus;
        _universe = universe;
    }
    public StarSystem GetStarSystem()
    {
        SpaceObjectController sp = _player.currentController;
        int gId = sp.sp_object.galaxyId;
        int sId = sp.sp_object.systemId;
        if (sp == null)
        {
            return null;
        }
        if (_starSystem != null && _starSystem.galaxyId == gId && _starSystem.id == sId)
        {
            return _starSystem;
        }
        StarSystem sps = _universe.FindSystem(gId, sId);
        _starSystem = sps;
        return sps;
    }
}
