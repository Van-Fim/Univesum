using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
[System.Serializable]
public class SpaceObjectData
{
    public int id;
    public string loadoutName;

    public int galaxyId;
    public int systemId;
    public int maxHull = 10000;
    public int hull;
    public int maxShield = 10000;
    public int shield;
    public int jobId;

    public Vector3 position = new Vector3();
    public Vector3 rotation = new Vector3();

    public bool is_destroyed;
    public bool is_initialized;
    public bool is_subscribed;
    public bool is_player;
    
    public SpaceObjectConfig spaceObjectConfig;
    public List<LoadoutHP> loadoutHPs;

    public virtual bool ReadData(SpaceObject spaceObject)
    {
        bool ret = false;
        if (!spaceObject)
        {
            return ret;
        }
        position = spaceObject.transform.localPosition;
        rotation = spaceObject.transform.localEulerAngles;
        id = spaceObject.id;
        galaxyId = spaceObject.galaxyId;
        systemId = spaceObject.systemId;
        jobId = spaceObject.jobId;
        loadoutName = spaceObject.loadoutName;
        maxHull = spaceObject.maxHull;
        hull = spaceObject.hull;
        maxShield = spaceObject.maxShield;
        shield = spaceObject.shield;
        spaceObjectConfig = spaceObject.spaceObjectConfig;
        loadoutHPs = spaceObject.loadoutHPs;
        is_destroyed = spaceObject.is_destroyed;
        is_initialized = spaceObject.is_initialized;
        is_subscribed = spaceObject.is_subscribed;
        is_player = spaceObject.is_player;
        ret = true;
        return ret;
    }
    public virtual bool InstallData(SpaceObject spaceObject)
    {
        bool ret = false;
        if (!spaceObject)
        {
            return ret;
        }
        spaceObject.transform.localPosition = position;
        spaceObject.transform.localEulerAngles = rotation;

        spaceObject.id = id;
        spaceObject.galaxyId = galaxyId;
        spaceObject.systemId = systemId;
        spaceObject.jobId = jobId;
        spaceObject.loadoutName = loadoutName;
        spaceObject.maxHull = maxHull;
        spaceObject.hull = hull;
        spaceObject.maxShield = maxShield;
        spaceObject.shield = shield;
        spaceObject.spaceObjectConfig = spaceObjectConfig;
        spaceObject.loadoutHPs = loadoutHPs;
        spaceObject.is_destroyed = is_destroyed;
        spaceObject.is_initialized = is_initialized;
        spaceObject.is_player = is_player;

        ret = true;
        return ret;
    }
}
public abstract class SpaceObject : MonoBehaviour
{
    public int id;
    public string spawnId;
    public int maxHull = 10000;
    public int hull;
    public int maxShield = 10000;
    public int shield;
    public int jobId = -1;
    public List<LoadoutHP> loadoutHPs;
    public SpaceObjectConfig spaceObjectConfig;
    public SpaceObjectController spaceObjectController;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    public TargetSelect targetSelect;
    [Inject] public LangManager _langManager;
    [Inject] public SignalBus signalBus;
    [Inject] public PlayerService playerService;
    [Inject] public CanvasController canvasController;
    [Inject] public CameraManager cameraManager;
    [Inject] public DiContainer _container;
    [Inject] public CursorRaycaster cursorRaycaster;
    [Inject] public Universe _universe;
    [Inject] public SpaceObjectFactory _spaceObjectFactory;
    [Inject] SpaceContainer spaceContainer;

    public EngineConfig engine;
    public PowerGenerator powerGenerator;
    [Inject] public WeaponFactory _weaponFactory;

    public List<Weapon> weapons = new List<Weapon>();

    public static UnityAction<Type> OnDestroyAllAction;

    TargetSelect targetSelectPrefab;

    public Rigidbody rigidbody;
    public Transform hardpoints;

    public string loadoutName;

    public int galaxyId;
    public int systemId;

    public bool is_initialized = false;
    public bool is_destroyed = false;
    public bool is_subscribed = false;
    public bool is_player;
    public bool Is_main_installed {
        get
        {
            return main != null; 
        }
    }

    protected Mesh mesh;

    protected GameObject main = null;
    public StarSystem StarSystem;
    public virtual void OnChunkFloatingOriginFixStart(SignalChunkFloatingOriginFixStart signal)
    {
        if (is_player || this is Asteroid)
        {
            return;
        }
        // transform.SetParent(spaceContainer.transform);
    }
    public virtual void OnChunkFloatingOriginFixEnd(SignalChunkFloatingOriginFixEnd signal)
    {
        if (is_player || this is Asteroid)
        {
            return;
        }
        // transform.SetParent(null);
    }
    public virtual SpaceObjectData Save()
    {
        SpaceObjectData spaceObjectData = new SpaceObjectData();
        spaceObjectData.ReadData(this);
        return spaceObjectData;
    }
    public virtual SpaceObject LoadData(SpaceObjectData spaceObjectData)
    {
        return null;
    }
    public StarSystem GetStarSystem()
    {
        return _universe.FindSystem(galaxyId, systemId);
    }
    public void InstallLoadout()
    {
        if (!hardpoints)
        {
            return;
        }
        var loadout = JsonConfigLoader.LoadFromFile<Loadout>(
                    "Loadouts/" + loadoutName
                );
        InstallLoadout(loadout);
    }
    public bool TryInstallConfig(StarSystem starSystem = null)
    {
        bool ret = false;
        if (starSystem == null)
        {
            starSystem = SetStarSystem(galaxyId, systemId);
        }
        if (starSystem == null || StarSystem == null)
            return ret;

        if (playerService._player_sp_object && (playerService._player_sp_object == this || GetType() == typeof(Asteroid)))
            return ret;

        if (!is_player)
        {
            transform.SetParent(spaceContainer.transform);
        }

        if (playerService.GetStarSystem() == starSystem)
        {
            if (starSystem == StarSystem)
            {
                ret = true;
                InstallConfig();

                // var loadout = JsonConfigLoader.LoadFromFile<Loadout>(
                //     "Loadouts/" + loadoutName
                // );
                // InstallLoadout(loadout);
            }
            else
            {
                if (is_player)
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
            if (is_player)
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

        if (!is_initialized)
        {
            hull = maxHull;
            shield = maxShield;
            is_initialized = true;
        }
        if (!is_subscribed)
        {
            signalBus.Subscribe<SpaceObjectOnTakeDamage>(OnTakeDamage);
            signalBus.Subscribe<SpaceObjectOnDestroyHide>(OnSpDestroyHide);
            signalBus.Subscribe<SpaceObjectOnDestroy>(OnSpDestroy);
            signalBus.Subscribe<SignalOnPlayerChangedSystem>(OnPlayerChangedSystem);
            signalBus.Subscribe<SignalChunkFloatingOriginFixStart>(OnChunkFloatingOriginFixStart);
            signalBus.Subscribe<SignalChunkFloatingOriginFixEnd>(OnChunkFloatingOriginFixEnd);
            OnDestroyAllAction += OnDestroyAll;
            is_subscribed = true;
        }
    }
    public virtual void Start()
    {
        SetTargetSelect();
    }
    public virtual void OnDestroyAll(Type type)
    {
        if (type == null)
        {
            if (playerService._player_sp_object == this)
            {
                cameraManager.GetMainCamera().transform.SetParent(null);
            }

            Destroy();
        }
        else if (type == typeof(Asteroid) && this is Asteroid)
        {
            if (playerService._player_sp_object == this)
            {
                cameraManager.GetMainCamera().transform.SetParent(null);
            }

            Destroy();
        }
    }
    public virtual void Update()
    {

    }
    public virtual StarSystem SetStarSystem(int galaxyId, int systemId)
    {
        this.galaxyId = galaxyId;
        this.systemId = systemId;
        StarSystem = GetStarSystem();
        return StarSystem;
    }
    public virtual void OnSpDestroyHide(SpaceObjectOnDestroyHide signal)
    {
        if (signal.target == this || signal.target == null)
        {

        }
    }
    public virtual void OnPlayerChangedSystem(SignalOnPlayerChangedSystem signal)
    {
        SetTargetSelect();
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
                InvokeDestroy(signal.attacker);
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
        cam.transform.localEulerAngles = Vector3.zero;
    }

    public virtual void InstallLoadout(Loadout loadout)
    {
        if (loadout.hardpoints == null)
        {
            return;
        }
        for (int i = 0; i < loadout.hardpoints.Count; i++)
        {
            LoadoutHP hp = loadout.hardpoints[i];
            Transform checkHP = hardpoints.Find(hp.hardpoint);
            if (checkHP || hp.hardpoint == "Engine" || hp.hardpoint == "PowerGenerator")
            {
                loadoutHPs.Add(hp);
            }
        }
    }
    public virtual void BuildLoadouts()
    {
        if (hardpoints == null)
        {
            return;
        }
        for (int i = 0; i < loadoutHPs.Count; i++)
        {
            LoadoutHP hp = loadoutHPs[i];
            Debug.Log($"{hp.hardpoint} {hp.item}");
        }
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

        if (gm)
        {
            main = GameObject.Instantiate<GameObject>(gm, transform);
        }
        else
        {
            main = new GameObject();
            main.transform.SetParent(transform);
        }
        main.transform.localPosition = Vector3.zero;
        main.transform.localEulerAngles = Vector3.zero;
        main.name = "MAIN";
        hardpoints = main.transform.Find("HARDPOINTS");
        if (!hardpoints)
        {
            hardpoints = new GameObject().transform;
            hardpoints.transform.SetParent(main.transform);
            hardpoints.name = "HARDPOINTS";
            Transform cam = new GameObject().transform;
            cam.transform.SetParent(hardpoints);
            cam.name = "HPCamera";
        }
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
    public virtual void SetTargetSelect()
    {
        if (is_player)
        {
            return;
        }
        if (targetSelect == null && playerService.GetStarSystem() == StarSystem)
        {
            targetSelectPrefab = Resources.Load<TargetSelect>("Prefabs/TargetSelect");
            targetSelect = GameObject.Instantiate<TargetSelect>(targetSelectPrefab);
            targetSelect._signalBus = signalBus;
            targetSelect.langManager = _langManager;
            targetSelect.canvasController = canvasController;
            targetSelect.playerService = playerService;
            targetSelect.cameraManager = cameraManager;
            targetSelect.SetSpObject(this);
            targetSelect.transform.SetParent(canvasController.transform);
        }
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
        signalBus.Unsubscribe<SignalChunkFloatingOriginFixStart>(OnChunkFloatingOriginFixStart);
        signalBus.Unsubscribe<SignalChunkFloatingOriginFixEnd>(OnChunkFloatingOriginFixEnd);
        OnDestroyAllAction -= OnDestroyAll;
        if (targetSelect)
        {
            targetSelect.Destroy();
        }
        Destroy(gameObject);
    }
    public static void InvokeDestroyAll(Type type = null)
    {
        OnDestroyAllAction?.Invoke(type);
    }
}
