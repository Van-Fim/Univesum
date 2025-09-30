using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TargetIndicator : MonoBehaviour
{
    public SpaceObject target;
    public Transform targetIcon;
    public Transform targetIconBody;
    public TargetClickHandler targetClickIndicator;
    public Image hullImage;
    public Image shieldImage;
    public TextMeshProUGUI distanceText;
    public RectTransform offscreenArrow;
    [Inject] private CameraManager cameraManager;
    private Camera mainCamera;
    public float edgeOffset = 50f;
    private float percentValue = 0.4f;
    public void SetTarget(SpaceObject spaceObject)
    {
        if (spaceObject == null)
        {
            target = spaceObject;
            return;
        }
        if (spaceObject.maxShield > 0)
        {
            shieldImage.gameObject.SetActive(true);
            shieldImage.fillAmount = spaceObject.shield / spaceObject.maxShield;
        }
        else
        {
            shieldImage.gameObject.SetActive(false);
        }
        if (spaceObject.maxHull > 0)
        {
            hullImage.gameObject.SetActive(true);
            hullImage.fillAmount = spaceObject.hull / spaceObject.maxHull;
        }
        else
        {
            hullImage.gameObject.SetActive(false);
        }
        target = spaceObject;
    }
    void Start()
    {
        targetClickIndicator.targetIndicator = this;
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
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(target.transform.position);
        float radius = percentValue;
        bool isVisible = screenPoint.z > 0 && Vector2.Distance(screenPoint, screenCenter) < radius;

        if (isVisible)
        {
            float dist = Vector3.Distance(mainCamera.transform.position, target.transform.position);
            float scaleFactor = 1 * (5000 / dist);
            if (scaleFactor < 0.45f)
            {
                scaleFactor = 0.45f;
            }
            if (scaleFactor > 0.7f)
            {
                shieldImage.sprite = Resources.Load<Sprite>("Textures/UI/shield_indicator_128");
                hullImage.sprite = Resources.Load<Sprite>("Textures/UI/hull_indicator_128");
                targetClickIndicator.image.sprite = Resources.Load<Sprite>("Textures/UI/target_indicator_128");
            }
            else
            {
                shieldImage.sprite = Resources.Load<Sprite>("Textures/UI/shield_indicator_64");
                hullImage.sprite = Resources.Load<Sprite>("Textures/UI/hull_indicator_64");
                targetClickIndicator.image.sprite = Resources.Load<Sprite>("Textures/UI/target_indicator_64");
            }
            if (scaleFactor > 1)
            {
                scaleFactor = 1;
            }
            screenPoint = mainCamera.WorldToScreenPoint(target.transform.position);
            screenPoint.z = 0;
            targetIconBody.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            targetIcon.position = screenPoint;
            targetIcon.gameObject.SetActive(true);
            offscreenArrow.gameObject.SetActive(false);
            Vector3 dTextPos = distanceText.rectTransform.localPosition;
            dTextPos.y = -90 * scaleFactor;
            distanceText.rectTransform.localPosition = dTextPos;
            distanceText.text = $"{(dist / 1000).ToString("F2", CultureInfo.InvariantCulture) } Km";
        }
        else
        {
            screenPoint = mainCamera.WorldToScreenPoint(target.transform.position);
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