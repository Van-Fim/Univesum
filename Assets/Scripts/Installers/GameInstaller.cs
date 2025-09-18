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

        Container.Bind<Player>().AsSingle();
        Container.Bind<PlayerService>().AsSingle();
        Container.Bind<CursorManager>().AsSingle();
        Container.Bind<CameraManager>().AsSingle();

        CanvasController canvasControllerVar = Container.InstantiatePrefab(canvasController).GetComponent<CanvasController>();
        Container.Bind<CanvasController>().FromInstance(canvasControllerVar).AsSingle();

        GameObject suitGO = Container.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/SuitPrefab"));
        Suit suit = suitGO.GetComponent<Suit>();
        SpaceObjectConfig suitConfig = JsonConfigLoader.LoadFromResources<SpaceObjectConfig>("Configs/SpaceObjects/Suit/Suit01");
        suit.InstallConfig(suitConfig);
        suit.InstallCamera();
        PlayerController playerControllerVar = suit.AddComponent<SuitController>();
        playerControllerVar._rigidbody = suit.rigidbody;
        playerControllerVar.canvasController = Container.Resolve<CanvasController>();
        playerControllerVar.cameraManager = Container.Resolve<CameraManager>();
        Container.Bind<PlayerController>().FromInstance(playerControllerVar).AsSingle();

        WorldChunkManager worldChunkManagerVar = Container.InstantiatePrefab(worldChunkManager).GetComponent<WorldChunkManager>();
        Container.Bind<WorldChunkManager>().FromInstance(worldChunkManagerVar).AsSingle();
        Container.Bind<GameStartedHandler>().AsSingle();

        Container.BindSignal<SignalChunkManagerReady>()
    .ToMethod<GameStartedHandler>(handler => handler.ChunkManagerReady)
    .FromResolve();
        Container.BindSignal<SignalGameStarted>()
        .ToMethod<GameStartedHandler>(handler => handler.HandleGameStarted)
        .FromResolve();
    }
}