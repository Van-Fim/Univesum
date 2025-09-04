using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public PlayerController playerController;
    public WorldChunkManager worldChunkManager;
    public CanvasController canvasController;

    public GameObject asteroidPrefab;
    public int initialSize = 50;

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

        Container.BindMemoryPool<Asteroid, Asteroid.Pool>()
    .WithInitialSize(initialSize)
    .FromComponentInNewPrefab(asteroidPrefab)
    .UnderTransformGroup("AsteroidPool");

        WorldChunkManager worldChunkManagerVar = Container.InstantiatePrefab(worldChunkManager).GetComponent<WorldChunkManager>();
        Container.Bind<WorldChunkManager>().FromInstance(worldChunkManagerVar).AsSingle();

        Container.BindSignal<SignalGameStarted>()
    .ToMethod<GameStartedHandler>(handler => handler.HandleGameStarted)
    .FromResolve();
    }
}