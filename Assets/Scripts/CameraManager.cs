using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class CameraManager
{
    private Camera mainCamera;
    private Camera mapCamera;
    public int mapType;
    public static UnityAction<int> OnMapSwitchAction;
    public static CameraManager singleton;
    public static bool isMapOpened;

    public void Init()
    {
        OnMapSwitchAction += OnMapSwitch;
        singleton = this;
    }
    public void OnMapSwitch(int type)
    {
        mapType = type;
        if (mapType == 0)
        {
            Universe.singleton.galaxies.gameObject.SetActive(true);
            Universe.singleton.systems.gameObject.SetActive(true);
            Universe.singleton.currentSystem.gameObject.SetActive(false);
        }
        else if (mapType == 1)
        {
            Universe.singleton.galaxies.gameObject.SetActive(false);
            Universe.singleton.systems.gameObject.SetActive(false);
            Universe.singleton.currentSystem.gameObject.SetActive(true);
        }
    }
    public static void InvokeMapSwitch(int type)
    {
        OnMapSwitchAction?.Invoke(type);
        MapSpaceUi.InvokeMapSwitch(type);
    }
    public Camera GetMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Prefabs/MainCamera"));
        }
        return mainCamera;
    }
    public Camera GetMapCamera()
    {
        if (mapCamera == null)
        {
            mapCamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Prefabs/MapCamera"));
        }
        return mapCamera;
    }
}