using UnityEngine;
using Zenject;

public class GameStartManager
{
    private readonly DiContainer _container;
    private readonly Player _player;
    private readonly SpaceObjectFactory _factory;
    private readonly GameStartConfig _config;

    public GameStartManager(
        DiContainer container,
        Player player,
        SpaceObjectFactory factory,
        [Inject(Id = "GameStartConfig")] GameStartConfig config)
    {
        _container = container;
        _player = player;
        _factory = factory;
        _config = config;
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(_config.ship))
        {
            CreateSuit();
        }
        else
        {
            CreateShip();
        }
    }

    private void CreateSuit()
    {
        Suit suit = _factory.Create<Suit>(
            "Prefabs/SuitPrefab",
            "Configs/SpaceObjects/Suit/Suit01"
        );

        suit.InstallCamera();

        var controller = suit.gameObject.AddComponent<SuitController>();
        _container.Inject(controller); 
        controller._rigidbody = suit.rigidbody;
        controller.sp_object = suit;
        suit.spaceObjectController = controller;

        _player.currentController = controller;
    }

    private void CreateShip()
    {
        Ship ship = _factory.Create<Ship>(
            "Prefabs/ShipPrefab",
            "Configs/SpaceObjects/Ships/" + _config.ship
        );



        var controller = ship.gameObject.AddComponent<PlayerShipController>();
        _container.Inject(controller); 
        controller._rigidbody = ship.rigidbody;
        controller.sp_object = ship;

        ship.spaceObjectController = controller;
        _player.currentController = controller;

        ship.InstallCamera();

        if (!string.IsNullOrEmpty(_config.ship_loadout))
        {
            var loadout = JsonConfigLoader.LoadFromResources<Loadout>(
                "Configs/Loadouts/" + _config.ship_loadout
            );
            ship.InstallLoadout(loadout);
        }
    }
}
