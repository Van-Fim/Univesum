using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class CameraManager
{
    private Camera mainCamera;
    private Camera mapCamera;
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