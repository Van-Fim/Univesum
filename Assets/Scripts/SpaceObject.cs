using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Zenject;
public abstract class SpaceObject : MonoBehaviour
{
    public int maxHull = 10000;
    public int hull;
    public int maxShield = 10000;
    public int shield;
    public SpaceObjectConfig spaceObjectConfig;
    public SpaceObjectController spaceObjectController;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    [Inject] public SignalBus signalBus;
    [Inject] public Player localPlayer;
    [Inject] public CanvasController canvasController;
    [Inject] public CameraManager cameraManager;
    [Inject] public DiContainer _container;
    [Inject] public CursorRaycaster cursorRaycaster;
    public Rigidbody rigidbody;
    public Transform hardpoints;

    bool is_initialized = false;
    bool is_destroyed = false;

    protected Mesh mesh;

    protected GameObject main = null;

    public virtual void Init()
    {
        hull = maxHull;
        shield = maxShield;
        Hide();

        if (!is_initialized)
        {
            signalBus.Subscribe<SpaceObjectOnTakeDamage>(OnTakeDamage);
            signalBus.Subscribe<SpaceObjectOnDestroy>(OnSpDestroy);
            is_initialized = true;
        }
    }

    public virtual void Update()
    {

    }
    public virtual void OnSpDestroy(SpaceObjectOnDestroy signal)
    {
        if (signal.target == this)
        {
            Destroy();
        }
    }
    public virtual void OnTakeDamage(SpaceObjectOnTakeDamage signal)
    {
        if (signal.target == this)
        {
            shield -= signal.value;
            if (shield < 0)
            {
                hull -= -shield;
                shield = 0;
            }
            if (hull <= 0)
            {
                hull = 0;
                InvokeDestroy(signal.attacker);
            }
        }
    }
    public virtual bool IsOwnedByLocalPlayer()
    {
        return spaceObjectController != null && spaceObjectController.IsOwnedByLocalPlayer(localPlayer);
    }
    public virtual void InstallCamera()
    {
        Transform camHardpoint = hardpoints.Find("HPCamera");
        Camera cam = cameraManager.GetMainCamera();
        cam.transform.SetParent(camHardpoint);
        cam.transform.localPosition = Vector3.zero;
        cam.transform.rotation = Quaternion.identity;
    }

    public virtual void InstallLoadout(Loadout loadout)
    {

    }
    public virtual void InvokeTakeDamage(SpaceObject attacker, int damage)
    {
        signalBus.Fire(new SpaceObjectOnTakeDamage(attacker, this, damage));
    }
    public virtual void InvokeDestroy(SpaceObject attacker)
    {
        signalBus.Fire(new SpaceObjectOnDestroy(attacker, this));
    }
    public virtual void InstallConfig(SpaceObjectConfig config)
    {
        if (this.main != null)
        {
            return;
        }
        spaceObjectConfig = config;
        if (rigidbody != null)
            rigidbody.mass = config.mass;

        GameObject gm = Resources.Load<GameObject>(config.pathToModel);
        if (config != null && config.chinldName != null && config.chinldName.Length > 0)
        {
            var tr = gm.transform.Find(config.chinldName);
            if (tr != null)
            {
                gm = tr.gameObject;
            }
        }
        main = GameObject.Instantiate<GameObject>(gm, transform);
        main.transform.localPosition = Vector3.zero;
        main.transform.localEulerAngles = Vector3.zero;
        main.name = "MAIN";
        hardpoints = main.transform.Find("HARDPOINTS");
        Transform fmain = main.transform.Find("MAIN");
        if (fmain != null)
        {
            fmain.transform.SetParent(transform);
            hardpoints.SetParent(transform);
            Destroy(main);
            main = fmain.gameObject;
        }
        gameObject.AddComponent<MeshCollider>();
        gameObject.AddComponent<Rigidbody>();
        meshRenderer = main.GetComponent<MeshRenderer>();
        main.transform.localScale = new Vector3(config.scale, config.scale, config.scale);
        if (meshRenderer == null)
        {
            meshRenderer = main.AddComponent<MeshRenderer>();
        }
        meshFilter = main.GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        if (meshFilter != null)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = config.mass;
        rigidbody.linearDamping = config.linearDrag;
        rigidbody.angularDamping = config.angularDrag;
        meshCollider.convex = true;
        rigidbody.useGravity = false;

        if (config.pathToMaterial != null && config.pathToMaterial.Length > 0)
        {
            Material mat = Resources.Load<Material>(config.pathToMaterial);
            meshRenderer.material = mat;
        }
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void Destroy()
    {
        if (is_destroyed)
        {
            return;
        }
        gameObject.SetActive(false);
        is_destroyed = true;
        signalBus.Unsubscribe<SpaceObjectOnTakeDamage>(OnTakeDamage);
        signalBus.Unsubscribe<SpaceObjectOnDestroy>(OnSpDestroy);
        Destroy(gameObject);
    }
}
