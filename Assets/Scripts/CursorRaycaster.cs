using UnityEngine;
using Zenject;
public class CursorRaycaster : MonoBehaviour
{
    [Inject] public CameraManager cameraManager;
    public LayerMask aimMask; // слои, куда можно целиться

    public Vector3 AimPoint { get; private set; }

    void Update()
    {
        Ray ray = cameraManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, aimMask))
        {
            AimPoint = hit.point;
        }
        else
        {
            AimPoint = ray.GetPoint(1000f); // точка вдаль, если ничего не попали
        }
    }
}
