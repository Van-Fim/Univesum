using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TargetIndicator : MonoBehaviour
{
    public Transform target;
    public RectTransform targetIcon;
    public RectTransform offscreenArrow;
    [Inject] private CameraManager cameraManager;
    private Camera mainCamera;
    public float edgeOffset = 50f;
    private float percentValue = 0.4f;
    void Start()
    {
        mainCamera = cameraManager.GetMainCamera();
    }
    void Update()
    {
        if (target == null)
        {
            targetIcon.gameObject.SetActive(false);
            offscreenArrow.gameObject.SetActive(false);
            return;
        }

        Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0); // в viewport-координатах
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(target.position);
        float radius = percentValue;
        bool isVisible = screenPoint.z > 0 && Vector2.Distance(screenPoint, screenCenter) < radius;

        if (isVisible)
        {
            screenPoint = mainCamera.WorldToScreenPoint(target.position);
            screenPoint.z = 0;
            targetIcon.position = screenPoint;
            targetIcon.gameObject.SetActive(true);
            offscreenArrow.gameObject.SetActive(false);
        }
        else
        {
            screenPoint = mainCamera.WorldToScreenPoint(target.position);
            if (screenPoint.z < 0)
            {
                screenPoint *= -1;
            }
            screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
            Vector3 direction = screenPoint - screenCenter;
            float angleRad = Mathf.Atan2(direction.y, direction.x);

            radius = Mathf.Min(Screen.width, Screen.height) * percentValue;
            Vector3 arrowPos = screenCenter + new Vector3(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius,
                0
            );

            offscreenArrow.position = arrowPos;
            offscreenArrow.rotation = Quaternion.Euler(0, 0, angleRad * Mathf.Rad2Deg - 90);


            targetIcon.gameObject.SetActive(false);
            offscreenArrow.gameObject.SetActive(true);
        }
    }
}