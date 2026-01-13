using UnityEngine;
using Zenject;

public class PSpace : MonoBehaviour
{
    private SignalBus _signalBus;
    public int safeRange = 10;
    public SpaceConfig config;
    [Inject]
    public virtual void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<SpaceShowSignal>(OnSpaceShow);
    }
    public void Destroy()
    {
        _signalBus.Unsubscribe<SpaceShowSignal>(OnSpaceShow);
        GameObject.Destroy(gameObject);
    }
    void OnSpaceShow(SpaceShowSignal signal)
    {
        Debug.Log(signal.space);
    }
    void Update()
    {

    }
}