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

        // Debug visualization for the ray
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            AimPoint = hit.point;
            // Draw a small sphere at the hit point
            Debug.DrawRay(hit.point, Vector3.up * 0.1f, Color.green);
        }
        else
        {
            AimPoint = ray.GetPoint(1000f); // точка вдаль, если ничего не попали
        }
    }
}
