using System;
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
    [Inject] public PlayerService playerService;
    [Inject] public CanvasController canvasController;
    [Inject] public CameraManager cameraManager;
    [Inject] public DiContainer _container;
    [Inject] public CursorRaycaster cursorRaycaster;
    [Inject] public Universe _universe;
    [Inject] SpaceContainer spaceContainer;

    public Rigidbody rigidbody;
    public Transform hardpoints;

    public string loadoutName;

    public int galaxyId;
    public int systemId;

    bool is_initialized = false;
    public bool is_destroyed = false;

    protected Mesh mesh;

    protected GameObject main = null;
    public StarSystem StarSystem;
    public StarSystem GetStarSystem()
    {
        return _universe.FindSystem(galaxyId, systemId);
    }
    public bool TryInstallConfig(StarSystem starSystem)
    {
        bool ret = false;
        if (starSystem == null || StarSystem == null)
            return ret;

        if (playerService._player_sp_object == this || GetType() == typeof(Asteroid))
            return ret;

        transform.SetParent(spaceContainer.transform);

        if (playerService.GetStarSystem() == starSystem)
        {
            if (starSystem == StarSystem)
            {
                ret = true;
                InstallConfig();
                var loadout = JsonConfigLoader.LoadFromFile<Loadout>(
                    "Loadouts/" + loadoutName
                );
                InstallLoadout(loadout);
            }
            else
            {
                ret = false;
                DestroyConfig();
                if (hardpoints)
                {
                    Destroy(hardpoints.gameObject);
                }
            }
        }
        else
        {
            ret = false;
            DestroyConfig();
            if (hardpoints)
            {
                Destroy(hardpoints.gameObject);
            }
        }

        return ret;
    }
    public virtual void Init()
    {
        if (is_destroyed)
            return;
        hull = maxHull;
        shield = maxShield;

        if (!is_initialized)
        {
            signalBus.Subscribe<SpaceObjectOnTakeDamage>(OnTakeDamage);
            signalBus.Subscribe<SpaceObjectOnDestroyHide>(OnSpDestroyHide);
            signalBus.Subscribe<SpaceObjectOnDestroy>(OnSpDestroy);
            signalBus.Subscribe<SignalOnPlayerChangedSystem>(OnPlayerChangedSystem);
            is_initialized = true;
        }
    }

    public virtual void Update()
    {

    }
    public virtual void SetStarSystem(int galaxyId, int systemId)
    {
        this.galaxyId = galaxyId;
        this.systemId = systemId;
        StarSystem = GetStarSystem();
    }
    public virtual void OnSpDestroyHide(SpaceObjectOnDestroyHide signal)
    {
        if (signal.target == this || signal.target == null)
        {

        }
    }
    public virtual void OnPlayerChangedSystem(SignalOnPlayerChangedSystem signal)
    {
        TryInstallConfig(signal.starSystem);
    }
    public virtual void OnSpDestroy(SpaceObjectOnDestroy signal)
    {
        if (signal.target == this || signal.target == null)
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
                InvokeDestroyHide(signal.attacker);
            }
        }
    }
    public virtual bool IsOwnedByLocalPlayer()
    {
        if (is_destroyed)
            return false;
        return spaceObjectController != null && spaceObjectController.IsOwnedByLocalPlayer(playerService._player);
    }
    public virtual void InstallCamera()
    {
        if (is_destroyed)
            return;
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
    public virtual void InvokeDestroyHide(SpaceObject attacker)
    {
        signalBus.Fire(new SpaceObjectOnDestroyHide(attacker, this));
    }
    public virtual void InstallConfig()
    {
        if (this.main != null)
        {
            return;
        }
        if (is_destroyed)
            return;

        if (rigidbody != null)
            rigidbody.mass = spaceObjectConfig.mass;

        GameObject gm = Resources.Load<GameObject>(spaceObjectConfig.pathToModel);
        if (spaceObjectConfig != null && spaceObjectConfig.chinldName != null && spaceObjectConfig.chinldName.Length > 0)
        {
            var tr = gm.transform.Find(spaceObjectConfig.chinldName);
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
        main.transform.localScale = new Vector3(spaceObjectConfig.scale, spaceObjectConfig.scale, spaceObjectConfig.scale);
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
        rigidbody.mass = spaceObjectConfig.mass;
        rigidbody.linearDamping = spaceObjectConfig.linearDrag;
        rigidbody.angularDamping = spaceObjectConfig.angularDrag;
        meshCollider.convex = true;
        rigidbody.useGravity = false;

        if (spaceObjectConfig.pathToMaterial != null && spaceObjectConfig.pathToMaterial.Length > 0)
        {
            Material mat = Resources.Load<Material>(spaceObjectConfig.pathToMaterial);
            meshRenderer.material = mat;
        }
    }

    public virtual void DestroyConfig()
    {
        if (this.main == null)
        {
            return;
        }
        if (is_destroyed)
            return;

        GameObject gm = Resources.Load<GameObject>(spaceObjectConfig.pathToModel);
        if (spaceObjectConfig != null && spaceObjectConfig.chinldName != null && spaceObjectConfig.chinldName.Length > 0)
        {
            var tr = gm.transform.Find(spaceObjectConfig.chinldName);
            if (tr != null)
            {
                gm = tr.gameObject;
            }
        }
        Destroy(main);
        if (meshCollider)
        {
            Destroy(meshCollider);
        }
        if (rigidbody)
        {
            Destroy(rigidbody);
        }
    }

    public virtual void Show()
    {
        if (is_destroyed)
            return;
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        if (is_destroyed)
            return;
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
        signalBus.Unsubscribe<SpaceObjectOnDestroyHide>(OnSpDestroyHide);
        signalBus.Unsubscribe<SpaceObjectOnDestroy>(OnSpDestroy);
        signalBus.Unsubscribe<SignalOnPlayerChangedSystem>(OnPlayerChangedSystem);
        Destroy(gameObject);
    }
}
