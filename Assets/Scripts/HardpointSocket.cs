using UnityEngine;

public class HardpointSocket : MonoBehaviour
{
    public enum HardpointType
    {
        Weapon,
        Turret,
        Engine,
        Camera,
        ShieldFX,
        Dock,
        UIAnchor,
        Custom
    }

    [Header("Тип точки")]
    public HardpointType type = HardpointType.Custom;

    [Header("Идентификатор точки (например HpWeapon01)")]
    public string socketID = "HpCustom";

    [Header("Дополнительные параметры")]
    public Vector3 localOffset;
    public Vector3 localRotation;

    public Transform GetWorldTransform()
    {
        // Возвращает позицию с учётом смещения
        var pos = transform.position + transform.TransformDirection(localOffset);
        var rot = transform.rotation * Quaternion.Euler(localRotation);
        var temp = new GameObject("TempSocketTransform");
        temp.transform.position = pos;
        temp.transform.rotation = rot;
        return temp.transform;
    }
}