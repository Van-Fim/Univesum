using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class TargetSelect : MonoBehaviour, IPointerClickHandler
{
    public GameObject main;
    public Image topLeft;
    public Image topRight;
    public Image bottomLeft;
    public Image bottomRight;
    public Image shieldBar;
    public Image hullBar;
    public Image offscreenArrow;
    public Image clickArea;
    public static UnityAction OnSelectAction;
    public static TargetSelect currentSelectedItem;
    public SpaceObject spaceObject;
    public static UnityAction OnTickAction;
    public PlayerService playerService;
    public SignalBus _signalBus;
    public LangManager langManager;
    public CanvasController canvasController;
    public CameraManager cameraManager;

    public static float scaleValue = 20;
    public static float posValue = 20;

    public float percentValue = 0.4f;

    byte type = 0;

    Vector2 barVals = new Vector2(120, 5);
    float barPoses = 70;

    bool is_destroyed;
    public virtual void SetSpObject(SpaceObject spaceObject)
    {
        if (spaceObject == null)
        {
            return;
        }
        this.spaceObject = spaceObject;
        this.spaceObject.targetSelect = this;
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedItem = this;
        InvokeSelect();
    }
    public virtual void Start()
    {
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);
        OnSelectAction += OnSelect;
        gameObject.SetActive(false);
        offscreenArrow.gameObject.SetActive(false);
        SwitchImage(0);
    }
    public virtual void SwitchImage(byte type)
    {
        this.type = type;
        if (this.type == 1)
        {
            Color32 col = shieldBar.color;
            shieldBar.color = new Color32(col.r, col.g, col.b, 255);

            col = hullBar.color;
            hullBar.color = new Color32(col.r, col.g, col.b, 255);

            col = topRight.color;
            topRight.color = new Color32(col.r, col.g, col.b, 255);

            col = topRight.color;
            topLeft.color = new Color32(col.r, col.g, col.b, 255);

            col = bottomRight.color;
            bottomRight.color = new Color32(col.r, col.g, col.b, 255);

            col = bottomLeft.color;
            bottomLeft.color = new Color32(col.r, col.g, col.b, 255);

            topRight.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/topRightLarge");
            topLeft.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/topLeftLarge");
            bottomRight.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/bottomRightLarge");
            bottomLeft.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/bottomLeftLarge");
        }
        else if (this.type == 0)
        {
            Color32 col = shieldBar.color;
            shieldBar.color = new Color32(col.r, col.g, col.b, 150);

            col = hullBar.color;
            hullBar.color = new Color32(col.r, col.g, col.b, 150);

            col = topRight.color;
            topRight.color = new Color32(col.r, col.g, col.b, 150);

            col = topRight.color;
            topLeft.color = new Color32(col.r, col.g, col.b, 150);

            col = bottomRight.color;
            bottomRight.color = new Color32(col.r, col.g, col.b, 150);

            col = bottomLeft.color;
            bottomLeft.color = new Color32(col.r, col.g, col.b, 150);

            topRight.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/topRight");
            topLeft.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/topLeft");
            bottomRight.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/bottomRight");
            bottomLeft.sprite = Resources.Load<Sprite>("Textures/UI/SelectBox/bottomLeft");
        }
    }
    public virtual void UpdateUiPos()
    {
        float dst = Vector3.Distance(playerService._player_sp_object.transform.position, spaceObject.transform.position);
        if (spaceObject is Station)
        {
            dst = Mathf.Clamp(20000 / dst, 0.5f, 1f);
        }
        else if (spaceObject is Ship)
        {
            dst = Mathf.Clamp(20000 / dst, 0.3f, 1f);
        }

        float ff = 1 + (1 / dst)*2;

        hullBar.fillAmount = (float)spaceObject.hull / spaceObject.maxHull;
        shieldBar.fillAmount = (float)spaceObject.shield / spaceObject.maxShield;

        topLeft.transform.localPosition = new Vector2(-posValue * dst, posValue * dst);
        topLeft.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scaleValue);
        topLeft.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaleValue);

        topRight.transform.localPosition = new Vector2(posValue * dst, posValue * dst);
        topRight.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scaleValue);
        topRight.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaleValue);

        bottomLeft.transform.localPosition = new Vector2(-posValue * dst, -posValue * dst);
        bottomLeft.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scaleValue);
        bottomLeft.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaleValue);

        bottomRight.transform.localPosition = new Vector2(posValue * dst, -posValue * dst);
        bottomRight.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scaleValue);
        bottomRight.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaleValue);
        hullBar.transform.localPosition = new Vector2(0, posValue * dst + posValue/2 + 5);
        hullBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, barVals.y);
        hullBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaleValue + posValue * dst * 2);
        shieldBar.transform.localPosition = new Vector2(0, hullBar.transform.localPosition.y + 7);
        shieldBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, barVals.y);
        shieldBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaleValue + posValue * dst * 2);
        
        clickArea.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, posValue * ff);
        clickArea.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, posValue * ff);
    }
    public virtual void Destroy()
    {
        OnSelectAction -= OnSelect;
        is_destroyed = true;
        Destroy(gameObject);
    }
    public virtual void OnSelect()
    {
        if (is_destroyed)
        {
            return;
        }
        if (currentSelectedItem != this)
        {
            SwitchImage(0);
        }
        else
        {
            SwitchImage(1);
        }
    }

    public virtual void OnUpdateTick()
    {
        if (is_destroyed)
        {
            return;
        }
        if (cameraManager.GetMapCamera().enabled)
        {
            gameObject.SetActive(false);
            return;
        }
        else if (cameraManager.GetMainCamera().enabled)
        {
            gameObject.SetActive(true);
        }
        if (spaceObject == null)
        {
            gameObject.SetActive(false);
            return;
        }
        if (spaceObject.StarSystem != playerService.GetStarSystem())
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (!cameraManager.GetMainCamera().enabled)
        {
            return;
        }
        Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0); // в viewport-координатах
        Vector3 screenVPPoint = cameraManager.GetMainCamera().WorldToViewportPoint(spaceObject.transform.position);
        Vector3 screenPoint = cameraManager.GetMainCamera().WorldToScreenPoint(spaceObject.transform.position);
        float radius = percentValue;
        bool isVisible = screenVPPoint.z > 0 && Vector2.Distance(screenVPPoint, screenCenter) < radius;
        if (screenPoint.z < 0 && TargetSelect.currentSelectedItem != this)
        {
            gameObject.SetActive(false);
            return;
        }
        if (TargetSelect.currentSelectedItem == this)
        {
            if (isVisible)
            {
                float dist = Vector3.Distance(cameraManager.GetMainCamera().transform.position, spaceObject.transform.position);
                screenPoint = cameraManager.GetMainCamera().WorldToScreenPoint(spaceObject.transform.position);
                screenPoint.z = 0;
                main.transform.position = screenPoint;
                main.SetActive(true);
                offscreenArrow.gameObject.SetActive(false);
                // distanceText.text = $"{(dist / 1000).ToString("F2", CultureInfo.InvariantCulture)} Km";
            }
            else
            {
                screenPoint = cameraManager.GetMainCamera().WorldToScreenPoint(spaceObject.transform.position);
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

                offscreenArrow.transform.position = arrowPos;
                offscreenArrow.transform.rotation = Quaternion.Euler(0, 0, angleRad * Mathf.Rad2Deg - 90);
                main.SetActive(false);
                offscreenArrow.gameObject.SetActive(true);
            }
        }
        else
        {
            main.SetActive(true);
            offscreenArrow.gameObject.SetActive(false);
            if (screenPoint.z < 0)
            {
                main.SetActive(false);
                offscreenArrow.gameObject.SetActive(false);
                return;
            }
            screenPoint.z = 0;
            main.transform.position = screenPoint;
        }

        UpdateUiPos();

    }
    public static void InvokeTick()
    {
        OnTickAction?.Invoke();
    }
    public static void InvokeSelect()
    {
        OnSelectAction?.Invoke();
    }
}
