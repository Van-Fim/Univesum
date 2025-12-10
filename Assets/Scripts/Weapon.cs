using UnityEngine;
using Zenject;
public class Weapon : MonoBehaviour
{
    public WeaponConfig _config;
    public Ship _parent;
    Transform baseTransform;
    Transform barrelTransform;
    float yAngLimit = 30f;
    float xAngLimit = 30f;
    float rotationSpeed = 7f;
    protected GameObject main = null;
    [Inject]
    SignalBus _signalBus;
    [Inject]
    public void Construct(Ship parent, WeaponConfig config, SignalBus signalBus)
    {
        _parent = parent;
        _config = config;

        _signalBus = signalBus;
        _signalBus.Subscribe<WeaponFiredSignal>(OnFire);
    }

    private void OnDestroy()
    {
        _signalBus.Unsubscribe<WeaponFiredSignal>(OnFire);
    }
    public void OnFire()
    {
        Fire();
    }
    public void SetTransforms()
    {
        this.baseTransform = main.transform.Find("BASE");
        this.barrelTransform = baseTransform.transform.Find("BARREL");
    }
    public void Update()
    {
        if (_parent != null && baseTransform != null && barrelTransform != null && _parent.IsOwnedByLocalPlayer())
        {
            // переводим AimPoint в локальные координаты корабля
            Vector3 localDir = _parent.transform.InverseTransformPoint(_parent.cursorRaycaster.AimPoint);
            localDir.y = 0;
            // --- Y (база) ---
            float targetYaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            targetYaw = Mathf.Clamp(targetYaw, -yAngLimit, yAngLimit);

            Quaternion yRot = Quaternion.Euler(0, targetYaw, 0);
            baseTransform.localRotation = Quaternion.Slerp(
                baseTransform.localRotation, yRot, Time.deltaTime * rotationSpeed
            );

            // --- X (ствол) ---
            // пересчёт направления после поворота базы
            Vector3 barrelLocalDir = baseTransform.transform.InverseTransformPoint(_parent.cursorRaycaster.AimPoint);
            float targetPitch = -Mathf.Atan2(barrelLocalDir.y, barrelLocalDir.z) * Mathf.Rad2Deg;
            targetPitch = Mathf.Clamp(targetPitch, -xAngLimit, 0);
            Quaternion xRot = Quaternion.Euler(targetPitch, 0, 0);
            barrelTransform.localRotation = Quaternion.Slerp(
                barrelTransform.localRotation, xRot, Time.deltaTime * rotationSpeed
            );
        }
    }
    public virtual void Fire()
    {

    }
    public virtual void InstallConfig()
    {
        if (this.main != null)
        {
            return;
        }
        GameObject gm = Resources.Load<GameObject>(_config.pathToModel);

        main = GameObject.Instantiate<GameObject>(gm, transform);
        main.transform.localPosition = Vector3.zero;
        main.transform.localEulerAngles = Vector3.zero;
        main.name = "MAIN";
        SetTransforms();
    }
}