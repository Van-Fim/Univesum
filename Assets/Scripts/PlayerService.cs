using UnityEngine;
using Zenject;

public class PlayerService
{
    public Player _player;
    public SpaceObject _player_sp_object;
    private readonly SignalBus _signalBus;
    private readonly Universe _universe;

    private Galaxy _galaxy;
    private StarSystem _starSystem;
    public static PlayerService singleton;
    [Inject] DiContainer container;
    [Inject] SpaceContainer _spaceContainer;
    [Inject]
    public PlayerService(Player player, SignalBus signalBus, Universe universe)
    {
        _player = player;
        _signalBus = signalBus;
        _universe = universe;
        singleton = this;
    }
    public bool IsInMenu()
    {
        return _player.isInMenu;
    }
    public void SetIsInMenu(bool value)
    {
        _player.isInMenu = value;
    }
    public void Warp(StarSystem starSystem, Vector3 position, Vector3 rotation)
    {
        Random.InitState($"{_universe.seed}{starSystem.galaxyId}{starSystem.id}".GetHashCode());
        SpaceObjectController sp = _player.currentController;

        int rndm = Random.Range(0, starSystem.config.skyboxes.Count);
        ChangeSkybox(starSystem.config.skyboxes[rndm]);
        _signalBus.Fire(new SpaceShowSignal(starSystem));
        _signalBus.Fire(new SpaceObjectOnDestroyHide(null, null));
        _signalBus.Fire(new SignalChunkDestroy());

        if (sp == null)
        {
            return;
        }
        sp.Warp(starSystem, position, rotation);
        // sp.Sp_object.SetStarSystem(starSystem.galaxyId, starSystem.id);
        // WorldChunkManager wcm = WorldChunkManager.singleton;
        // if (wcm)
        // {
        //     wcm.Init();
        //     wcm.isFirstChunksReady = false;
        //     wcm.UpdateCurrentChunk();
        //     wcm.UpdateChunksAround(wcm.currentChunk);
        //     wcm.isFirstChunksReady = true;
        // }
        // _spaceContainer.transform.localPosition = Vector3.zero;
        // TargetSelect.currentSelectedItem = null;
        // TargetSelect.InvokeSelect();
        // _signalBus.Fire(new SignalOnPlayerChangedSystem(sp.Sp_object, starSystem));
    }

    public void ChangeSkybox(SpaceConfigSkybox skyboxConfig)
    {
        Material skyboxMaterial = Resources.Load<Material>($"Materials/Skybox/{skyboxConfig.name}");
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", skyboxConfig.color);
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }
    public Galaxy GetGalaxy()
    {
        SpaceObjectController sp = _player.currentController;
        int gId = sp.Sp_object.galaxyId;
        int sId = sp.Sp_object.systemId;
        if (sp == null)
        {
            return null;
        }
        if (_galaxy != null && _galaxy.id == gId)
        {
            return _galaxy;
        }
        Galaxy g = _universe.FindGalaxy(gId);
        _galaxy = g;
        return _galaxy;
    }
    public void SetStarSystem(StarSystem starSystem)
    {
        SpaceObjectController sp = _player.currentController;
        _starSystem = starSystem;
        if (!sp)
        {
            return;
        }
        sp.Sp_object.galaxyId = starSystem.galaxyId;
        sp.Sp_object.systemId = starSystem.id;
    }
    public StarSystem GetStarSystem()
    {
        SpaceObjectController sp = _player.currentController;
        if (!sp)
        {
            return null;
        }
        int gId = sp.Sp_object.galaxyId;
        int sId = sp.Sp_object.systemId;

        if (_starSystem != null && _starSystem.galaxyId == gId && _starSystem.id == sId)
        {
            return _starSystem;
        }
        StarSystem sps = _universe.FindSystem(gId, sId);
        _starSystem = sps;
        return sps;
    }
}
