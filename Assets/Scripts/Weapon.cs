using UnityEngine;
using Zenject;
public class Weapon : MonoBehaviour
{
    public WeaponConfig _config;
    public ProjectileConfig projectileConfig;
    public AudioSource audioSource;
    public SpaceObject _parent;
    public Transform baseTransform;
    public Transform barrelTransform;
    public Transform firePointTransform;
    float yAngLimit = 40f;
    float xAngLimit = 40f;
    float rotationSpeed = 7f;
    protected GameObject main = null;
    protected float _nextFireTime;
    private DiContainer container;
    [Inject]
    public CameraManager _cameraManager;
    [Inject]
    public PlayerService _playerService;
    [Inject]
    public SignalBus _signalBus;
    public ProjectilePool _pool;
    [Inject]
    private ProjectilePoolFactory poolFactory;
    [Inject]
    public virtual void Construct(SpaceObject parent, WeaponConfig config, SignalBus signalBus, DiContainer container)
    {
        _parent = parent;
        _config = config;

        _signalBus = signalBus;
        _signalBus.Subscribe<WeaponFiredSignal>(OnFire);
        projectileConfig = JsonConfigLoader.LoadFromFile<ProjectileConfig>("Projectiles/" + _config.projectile);
        this.container = container;
        _pool = container.ResolveId<ProjectilePool>(projectileConfig.name);
    }
    public virtual void Init()
    {
        
    }
    private void OnDestroy()
    {
        _signalBus.Unsubscribe<WeaponFiredSignal>(OnFire);
    }
    public virtual void OnFire(WeaponFiredSignal signal)
    {
        if (signal.spaceObject != _parent)
            return;
        TryFire();
    }
    public virtual void SetTransforms()
    {
        this.barrelTransform = baseTransform.transform.Find("BARREL");
        this.firePointTransform = new GameObject().transform;
        this.firePointTransform.SetParent(barrelTransform);
        this.firePointTransform.localPosition = new Vector3(0, 0.039218f, 0.773041f);
        this.firePointTransform.localRotation = Quaternion.identity;
    }
    public virtual void InstallAS()
    {
        audioSource = firePointTransform.gameObject.AddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Sounds/Weapons/" + _config.fireSound);
        audioSource.spatialBlend = 1;
        audioSource.dopplerLevel = 0;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 4f;
        audioSource.volume = 0.2f;
    }
    public virtual void Update()
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
        _pool.Spawn(this, projectileConfig, firePointTransform.position, _parent.cursorRaycaster.AimPoint, firePointTransform.rotation);
        audioSource.Play();
    }
    private void TryFire()
    {
        if (_playerService._player_sp_object == _parent && _parent)
        {
            if (_cameraManager.GetMapCamera().enabled)
            {
                return;
            }
        }
        if (Time.time >= _nextFireTime && _parent is Ship)
        {
            Ship pship = (Ship)_parent;
            PowerGenerator powerGenerator = pship.powerGenerator;
            if (powerGenerator.TryConsume(_config.energyCost))
            {
                powerGenerator.currentEnergy -= _config.energyCost;
                Fire();
                _nextFireTime = Time.time + (1f / _config.fireRate);
            }
        }
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
        this.baseTransform = main.transform;
        SetTransforms();
        InstallAS();
    }
}