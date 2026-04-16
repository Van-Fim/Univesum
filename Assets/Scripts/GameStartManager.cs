using UnityEngine;
using Zenject;

public class GameStartManager
{
    private readonly DiContainer _container;
    private readonly PlayerService _playerService;
    private readonly Universe _universe;
    private readonly SpaceObjectFactory _factory;
    private readonly SignalBus _signalBus;
    private readonly GameStartConfig _config;
    [Inject] private readonly FactionsManager _factionsManager;
    [Inject] private readonly LangManager _langManager;
    [Inject] private readonly CanvasController _canvasController;
    private NpcJobManager _npcJobManager;

    public GameStartManager(
        DiContainer container,
        PlayerService playerService,
        Universe universe,
        SpaceObjectFactory factory,
        SignalBus signalBus,
        NpcJobManager npcJobManager,
        [Inject(Id = "GameStartConfig")] GameStartConfig config)
    {
        _container = container;
        _universe = universe;
        _playerService = playerService;
        _factory = factory;
        _signalBus = signalBus;
        _config = config;
        _npcJobManager = npcJobManager;
    }

    public void Load()
    {
        Random.InitState(_universe.seed);
        _canvasController.Init();
        _langManager.Init();
        _factionsManager.Initialize();
        
        _universe.config = JsonConfigLoader.LoadFromFile<SpaceConfig>($"Universe/{_config.univesrse}");
        _universe.Init();
        _universe.Build();

        _npcJobManager.Initialize();

        if (string.IsNullOrEmpty(_config.ship))
        {
            CreateSuit();
        }
        else
        {
            CreateShip();
        }
        StarSystem sys = _universe.FindSystem(_config.galaxyId, _config.systemId);
        _playerService.Warp(sys, _config.start_position, _config.start_rotation);
    }

    private void CreateSuit()
    {
        Suit suit = _factory.Create<Suit>(
            "Prefabs/SuitPrefab",
            "SpaceObjects/Suit/Suit01"
        );

        suit.InstallCamera();

        var controller = suit.gameObject.AddComponent<SuitController>();
        _container.Inject(controller);
        controller._rigidbody = suit.rigidbody;
        controller.Sp_object = suit;
        suit.spaceObjectController = controller;

        _playerService._player.currentController = controller;

        suit.transform.localPosition = _config.start_position;
        suit.transform.localEulerAngles = _config.start_position;
        suit.SetStarSystem(_config.galaxyId, _config.systemId);
    }

    private void CreateShip()
    {
        Ship ship = _factory.Create<Ship>(
            "Prefabs/ShipPrefab",
            "SpaceObjects/Ships/" + _config.ship
        );
        ship.InstallConfig();
        var controller = ship.gameObject.AddComponent<PlayerShipController>();
        _container.Inject(controller);
        controller._rigidbody = ship.rigidbody;
        controller.Sp_object = ship;

        ship.spaceObjectController = controller;
        _playerService._player.currentController = controller;

        ship.InstallCamera();

        if (!string.IsNullOrEmpty(_config.ship_loadout))
        {
            ship.loadoutName = _config.ship_loadout;
            var loadout = JsonConfigLoader.LoadFromFile<Loadout>(
                "Loadouts/" + ship.loadoutName
            );
            ship.InstallLoadout(loadout);
        }

        ship.transform.localPosition = _config.start_position;
        ship.transform.localEulerAngles = _config.start_position;
        ship.SetStarSystem(_config.galaxyId, _config.systemId);
        ship.Init();
    }
}
