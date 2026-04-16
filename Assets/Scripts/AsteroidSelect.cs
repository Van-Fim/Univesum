using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
public class AsteroidSelect : TargetSelect
{
    public Image targetClickIndicator;

    public float edgeOffset = 50f;
    
    public override void Start()
    {
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);
        main.SetActive(false);
        offscreenArrow.gameObject.SetActive(false);

        shieldBar.sprite = Resources.Load<Sprite>("Textures/UI/shield_indicator_64");
        hullBar.sprite = Resources.Load<Sprite>("Textures/UI/hull_indicator_64");
        targetClickIndicator.sprite = Resources.Load<Sprite>("Textures/UI/target_indicator_64");
    }
    public override void OnUpdateTick()
    {
        if (spaceObject == null || !spaceObject.gameObject.activeSelf)
        {
            main.SetActive(false);
            offscreenArrow.gameObject.SetActive(false);
            return;
        }
        if (TargetSelect.currentSelectedItem && TargetSelect.currentSelectedItem != this)
        {
            main.SetActive(false);
            offscreenArrow.gameObject.SetActive(false);

            return;
        }
        if (spaceObject.maxShield == 0)
        {
            shieldBar.fillAmount = 0;
        }
        Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0); // в viewport-координатах
        Vector3 screenPoint = cameraManager.GetMainCamera().WorldToViewportPoint(spaceObject.transform.position);
        float radius = percentValue;
        bool isVisible = screenPoint.z > 0 && Vector2.Distance(screenPoint, screenCenter) < radius;

        if (isVisible)
        {
            float dist = Vector3.Distance(cameraManager.GetMainCamera().transform.position, spaceObject.transform.position);
            float scaleFactor = 0.60f;
            screenPoint = cameraManager.GetMainCamera().WorldToScreenPoint(spaceObject.transform.position);
            screenPoint.z = 0;
            main.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            main.transform.position = screenPoint;
            main.SetActive(true);
            offscreenArrow.gameObject.SetActive(false);

            // distanceText.text = $"{(dist / 1000).ToString("F2", CultureInfo.InvariantCulture)} Km";
            if (spaceObject.maxShield > 0)
            {
                shieldBar.fillAmount = (float)spaceObject.shield / (float)spaceObject.maxShield;
            }
            hullBar.fillAmount = (float)spaceObject.hull / (float)spaceObject.maxHull;
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
}