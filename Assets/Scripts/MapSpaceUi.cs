using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class MapSpaceUi : MonoBehaviour, IPointerClickHandler
{
    public Image image;
    public Camera cam;
    public PSpace space;
    public static UnityAction OnTickAction;
    public static UnityAction OnSelectAction;
    public static MapSpaceUi currentSelectedItem;
    public static Color32 currentColor = new Color32(0, 100, 0, 255);
    public static Color32 selectedColor = new Color32(255, 255, 255, 255);
    public static Color32 defaultColor = new Color32(50, 50, 50, 255);
    public PlayerService playerService;
    public SignalBus _signalBus;
    public void OnPointerClick(PointerEventData eventData)
    {
        currentSelectedItem = this;
        OnSelectAction?.Invoke();
    }
    public void Start()
    {
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);
        OnSelectAction += OnSelect;

        image = GetComponent<Image>();
        Color32 c = image.color;
        image.color = defaultColor;
    }
    public virtual void Destroy()
    {
        OnSelectAction -= OnSelect;
    }
    public void OnSelect()
    {
        ChangeColor();
    }
    public void ChangeColor()
    {
        if (playerService.GetStarSystem() == space)
        {
            image.color = currentColor;
            return;
        }
        if (currentSelectedItem != this)
        {
            Color32 c = image.color;
            image.color = defaultColor;
        }
        else
        {
            Color32 c = image.color;
            image.color = selectedColor;
        }
    }
    public void OnUpdateTick()
    {
        ChangeColor();
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (!cam.enabled)
        {
            return;
        }
        transform.position = cam.WorldToScreenPoint(space.transform.position);
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
