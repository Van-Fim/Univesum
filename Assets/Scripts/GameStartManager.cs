using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameStartManager
{
    private readonly DiContainer _container;
    private readonly PlayerService _playerService;
    private readonly Universe _universe;
    private readonly SpaceObjectFactory _factory;
    private readonly SignalBus _signalBus;
    private GameStartConfig _config;
    [Inject] private readonly FactionsManager _factionsManager;
    [Inject] private readonly LangManager _langManager;
    [Inject] private readonly CanvasController _canvasController;
    [Inject] private readonly CameraManager _cameraManager;
    [Inject] private readonly SaveManager _saveManager;
    public static GameStartManager singleton;
    private NpcJobManager _npcJobManager;
    public string startsPath;

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

        singleton = this;
    }
    public void SetConfig(GameStartConfig gameStartConfig)
    {
        _config = gameStartConfig;
    }
    public void Init()
    {
        startsPath = JsonConfigLoader.ConfigPath + "/" + "Gamestarts";

        Random.InitState(_universe.seed);
        _canvasController.Init();
        _langManager.Init();
        _factionsManager.Initialize();
        _universe.Init();
        _npcJobManager.Initialize();
        _saveManager.Init();
    }
    public void Load()
    {
        Random.InitState(_universe.seed);

        _universe.config = JsonConfigLoader.LoadFromFile<SpaceConfig>($"Universe/{_config.univesrse}");
        _universe.Build();

        _npcJobManager.Load();

        if (string.IsNullOrEmpty(_config.suit) && string.IsNullOrEmpty(_config.ship))
        {
            _canvasController.HideUi();
            CreateShip();
            _playerService.SetIsInMenu(true);
        }
        else if (!string.IsNullOrEmpty(_config.ship))
        {
            CreateShip();
        }
        else if (!string.IsNullOrEmpty(_config.suit))
        {
            CreateSuit();
        }
        StarSystem sys = _universe.FindSystem(_config.galaxyId, _config.systemId);
        _playerService.Warp(sys, _config.start_position, _config.start_rotation);
    }
    public List<string> GetAllStarts()
    {
        List<string> list = FolderLister.GetDirFiles(startsPath);
        return list;
    }
    private void CreateSuit()
    {
        Suit suit = _factory.Create<Suit>(
            "Prefabs/SuitPrefab",
            "SpaceObjects/Suit/Suit01"
        );

        suit.InstallCamera();

        var controller = _container.TryResolve<SuitController>();
        if (!controller)
        {
            controller = suit.gameObject.AddComponent<SuitController>();
            _container.Inject(controller);
        }
        controller._rigidbody = suit.rigidbody;
        controller.Sp_object = suit;
        suit.spaceObjectController = controller;

        _playerService._player.currentController = controller;
        _playerService._player_sp_object = suit;

        suit.transform.localPosition = _config.start_position;
        suit.transform.localEulerAngles = _config.start_position;
        suit.SetStarSystem(_config.galaxyId, _config.systemId);
    }

    private void CreateShip()
    {
        string cfg = "SpaceObjects/Ships/" + _config.ship;
        if (string.IsNullOrEmpty(_config.ship))
        {
            cfg = null;
        }
        Ship ship = _factory.Create<Ship>(
            "Prefabs/ShipPrefab",
            cfg
        );
        ship.InstallConfig();
        var controller = _container.TryResolve<PlayerShipController>();
        if (!controller)
        {
            controller = ship.gameObject.AddComponent<PlayerShipController>();
            _container.Inject(controller);
        }
        controller._rigidbody = ship.rigidbody;
        controller.Sp_object = ship;
        ship.is_player = true;

        ship.spaceObjectController = controller;
        _playerService._player.currentController = controller;
        _playerService._player_sp_object = ship;

        ship.InstallCamera();

        if (!string.IsNullOrEmpty(_config.ship_loadout))
        {
            ship.loadoutName = _config.ship_loadout;
            var loadout = JsonConfigLoader.LoadFromFile<Loadout>(
                "Loadouts/" + ship.loadoutName
            );
            ship.InstallLoadout(loadout);
        }
        ship.BuildLoadouts();
        ship.transform.localPosition = _config.start_position;
        ship.transform.localEulerAngles = _config.start_position;
        ship.SetStarSystem(_config.galaxyId, _config.systemId);
        ship.Init();
    }
}
