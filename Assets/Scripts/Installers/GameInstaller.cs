using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public WorldChunkManager worldChunkManager;
    public CanvasController canvasController;

    public override void InstallBindings()
    {
        Application.targetFrameRate = 70;
        SignalBusInstaller.Install(Container);

        Container.DeclareSignal<SignalDestroyChunkAsteroids>();
        Container.DeclareSignal<SignalGameStarted>();
        Container.DeclareSignal<SignalChunkManagerReady>();
        Container.DeclareSignal<SignalChunkFloatingOriginFix>();
        Container.DeclareSignal<PlayerSpeedChangedSignal>();
        Container.DeclareSignal<SpaceObjectOnTakeDamage>();
        Container.DeclareSignal<WeaponFiredSignal>();

        Container.Bind<Player>().AsSingle();
        Container.Bind<PlayerService>().AsSingle();
        Container.Bind<CameraManager>().AsSingle();
        Container.Bind<SpaceObjectFactory>().AsSingle();
        SpaceObjectFactory spaceObjectFactory = Container.Resolve<SpaceObjectFactory>();
        CursorManager cursorManagerVar = Container.InstantiateComponent<CursorManager>(new GameObject());
        cursorManagerVar.gameObject.name = "CursorManager";

        CursorRaycaster cursorRaycaster = Container.InstantiateComponent<CursorRaycaster>(new GameObject());
        cursorRaycaster.gameObject.name = "CursorRaycaster";

        Container.Bind<CursorManager>().FromInstance(cursorManagerVar).AsSingle();
        Container.Bind<CursorRaycaster>().FromInstance(cursorRaycaster).AsSingle();

        CanvasController canvasControllerVar = Container.InstantiatePrefab(canvasController).GetComponent<CanvasController>();
        Container.Bind<CanvasController>().FromInstance(canvasControllerVar).AsSingle();
        TargetIndicator targetIndicator = canvasControllerVar.gameObject.GetComponent<TargetIndicator>();
        Container.Bind<TargetIndicator>().FromInstance(targetIndicator).AsSingle();

        var startConfig = JsonConfigLoader.LoadFromResources<GameStartConfig>("Configs/Gamestarts/Default");
        Container.Bind<GameStartConfig>().WithId("GameStartConfig").FromInstance(startConfig).AsSingle();
        Container.Bind<GameStartManager>().AsSingle();
        Container.Resolve<GameStartManager>().Load();

        WorldChunkManager worldChunkManagerVar = Container.InstantiatePrefab(worldChunkManager).GetComponent<WorldChunkManager>();
        Container.Bind<WorldChunkManager>().FromInstance(worldChunkManagerVar).AsSingle();
        Container.Bind<GameStartedHandler>().AsSingle();

        Container.BindSignal<SignalChunkManagerReady>()
    .ToMethod<GameStartedHandler>(handler => handler.ChunkManagerReady)
    .FromResolve();
        Container.BindSignal<SignalGameStarted>()
        .ToMethod<GameStartedHandler>(handler => handler.HandleGameStarted)
        .FromResolve();

        //     PlayerController playerControllerVar = null;
        //     Player player = Container.Resolve<Player>();
        //     if (startConfig.ship == null || startConfig.ship.Length == 0)
        //     {
        //         GameObject suitGO = Container.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/SuitPrefab"));
        //         Suit suit = suitGO.GetComponent<Suit>();
        //         SpaceObjectConfig suitConfig = JsonConfigLoader.LoadFromResources<SpaceObjectConfig>("Configs/SpaceObjects/Suit/Suit01");
        //         suit.InstallConfig(suitConfig);
        //         suit.InstallCamera();
        //         playerControllerVar = suit.AddComponent<SuitController>();
        //         playerControllerVar._rigidbody = suit.rigidbody;
        //         playerControllerVar.sp_object = suit;
        //         Container.Bind<PlayerController>().FromInstance(playerControllerVar).AsSingle();
        //         player.currentController = playerControllerVar;
        //     }
        //     else
        //     {
        //         GameObject shipGO = Container.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/ShipPrefab"));
        //         Ship ship = shipGO.GetComponent<Ship>();
        //         SpaceObjectConfig shipConfig = JsonConfigLoader.LoadFromResources<SpaceObjectConfig>("Configs/SpaceObjects/Ships/" + startConfig.ship);

        //         ship.InstallConfig(shipConfig);
        //         ship.InstallCamera();

        //         if (startConfig.ship_loadout != null && startConfig.ship_loadout.Length > 0)
        //         {
        //             Loadout loadout = JsonConfigLoader.LoadFromResources<Loadout>("Configs/Loadouts/" + startConfig.ship_loadout);
        //             ship.InstallLoadout(loadout);
        //         }

        //         playerControllerVar = ship.AddComponent<PlayerShipController>();
        //         playerControllerVar._rigidbody = ship.rigidbody;
        //         playerControllerVar.sp_object = ship;
        //         Container.Bind<PlayerController>().FromInstance(playerControllerVar).AsSingle();
        //         player.currentController = playerControllerVar;
        //     }

        //     WorldChunkManager worldChunkManagerVar = Container.InstantiatePrefab(worldChunkManager).GetComponent<WorldChunkManager>();
        //     Container.Bind<WorldChunkManager>().FromInstance(worldChunkManagerVar).AsSingle();
        //     Container.Bind<GameStartedHandler>().AsSingle();

        //     Container.BindSignal<SignalChunkManagerReady>()
        // .ToMethod<GameStartedHandler>(handler => handler.ChunkManagerReady)
        // .FromResolve();
        //     Container.BindSignal<SignalGameStarted>()
        //     .ToMethod<GameStartedHandler>(handler => handler.HandleGameStarted)
        //     .FromResolve();
    }
}