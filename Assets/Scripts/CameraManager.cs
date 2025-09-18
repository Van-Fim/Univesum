using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class CameraManager
{
    private Camera mainCamera;
    public Camera GetMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Prefabs/MainCamera"));
        }
        return mainCamera;
    }
}