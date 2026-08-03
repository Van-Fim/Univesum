using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Zenject;
public class UpdateManager : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    public void Update()
    {
        _signalBus.Fire<SignalOnUpdateTick>();
        SpaceObject.InvokeTick();
        SpaceObjectController.InvokeTick();
    }
}
