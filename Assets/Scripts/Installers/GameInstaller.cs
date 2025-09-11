using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public PlayerController playerController;
    public WorldChunkManager worldChunkManager;
    public CanvasController canvasController;

    public override void InstallBindings()
    {
        Application.targetFrameRate = 70;
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<SignalDestroyChunkAsteroids>();
        Container.DeclareSignal<SignalGameStarted>();

        Container.Bind<Player>().AsSingle();
        Container.Bind<PlayerService>().AsSingle();

        CanvasController canvasControllerVar = Container.InstantiatePrefab(canvasController).GetComponent<CanvasController>();
        Container.Bind<CanvasController>().FromInstance(canvasControllerVar).AsSingle();

        PlayerController playerControllerVar = Container.InstantiatePrefab(playerController).GetComponent<PlayerController>();
        Container.Bind<PlayerController>().FromInstance(playerControllerVar).AsSingle();

        WorldChunkManager worldChunkManagerVar = Container.InstantiatePrefab(worldChunkManager).GetComponent<WorldChunkManager>();
        Container.Bind<WorldChunkManager>().FromInstance(worldChunkManagerVar).AsSingle();

        Container.Bind<GameStartedHandler>().AsSingle();
        Container.BindSignal<SignalGameStarted>()
    .ToMethod<GameStartedHandler>(handler => handler.HandleGameStarted)
    .FromResolve();
    }
}