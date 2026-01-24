using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PSpace : MonoBehaviour
{
    public int id;
    private SignalBus _signalBus;
    public Universe _universe;
    public StarSystem.Factory _starSystemFactory;
    public int safeRange = 10;
    public SpaceConfig config;
    public List<AsteroidFieldConfig> _asteroidConfigs;
    [Inject]
    public virtual void Construct(SignalBus signalBus, Universe universe, StarSystem.Factory starSystemFactory, List<AsteroidFieldConfig> asteroidFieldConfigs)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<SpaceShowSignal>(OnSpaceShow);
        _universe = universe;
        _starSystemFactory = starSystemFactory;
        _asteroidConfigs = asteroidFieldConfigs;
    }
    public void Destroy()
    {
        _signalBus.Unsubscribe<SpaceShowSignal>(OnSpaceShow);
        GameObject.Destroy(gameObject);
    }
    void OnSpaceShow(SpaceShowSignal signal)
    {
        
    }
    void Update()
    {

    }
}