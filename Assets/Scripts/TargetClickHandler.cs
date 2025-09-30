using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TargetClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Image image;
    public TargetIndicator targetIndicator;
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{targetIndicator.target} selected");
    }
}