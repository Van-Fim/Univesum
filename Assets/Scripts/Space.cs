using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class PSpace : MonoBehaviour
{
    public int id;
    private SignalBus _signalBus;
    public Universe _universe;
    public StarSystem.Factory _starSystemFactory;
    public CanvasController _canvas;
    public int safeRange = 10;
    public SpaceConfig config;
    public List<AsteroidFieldConfig> _asteroidConfigs;
    public byte[] color = new byte[] { 255, 255, 255, 255 };

    public Faction faction;

    public MapSpaceUi MapSpaceUiPrefab;
    public MapSpaceUi mapSpaceUi;

    public static UnityAction<Type> OnDestroyAllAction;

    [Inject] public PlayerService _playerService;
    [Inject] public CameraManager _cameraManager;
    [Inject] public FactionsManager _factionsManager;

    public void Start()
    {
        MapSpaceUiPrefab = Resources.Load<MapSpaceUi>("Prefabs/MapSpaceUi");
        mapSpaceUi = Instantiate<MapSpaceUi>(MapSpaceUiPrefab);
        mapSpaceUi._signalBus = _signalBus;
        mapSpaceUi.transform.SetParent(_canvas.transform);
        mapSpaceUi.gameObject.SetActive(false);
        mapSpaceUi.cam = _cameraManager.GetMapCamera();
        mapSpaceUi.playerService = _playerService;
        mapSpaceUi.space = this;

        OnDestroyAllAction += OnDestroyAll;
    }
    public virtual void OnDestroyAll(Type type)
    {
        if (type == null)
        {
            Destroy();
        }
        else if (type == this.GetType())
        {
            Destroy();
        }
    }
    public static void InvokeDestroyAll(Type type = null)
    {
        OnDestroyAllAction?.Invoke(type);
    }
    public virtual void OnMinimapRender()
    {
        Camera mapCam = _cameraManager.GetMapCamera();
        bool v = mapCam.enabled;
        if (v)
        {
            mapSpaceUi.gameObject.SetActive(true);
        }
        else
        {
            mapSpaceUi.gameObject.SetActive(false);
        }
    }
    [Inject]
    public virtual void Construct(SignalBus signalBus, Universe universe, StarSystem.Factory starSystemFactory, List<AsteroidFieldConfig> asteroidFieldConfigs, CanvasController canvas)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<SpaceShowSignal>(OnSpaceShow);
        _signalBus.Subscribe<SpaceOnMinimapRenderSignal>(OnMinimapRender);
        _universe = universe;
        _starSystemFactory = starSystemFactory;
        _asteroidConfigs = asteroidFieldConfigs;
        _canvas = canvas;
    }
    public void Destroy()
    {
        _signalBus.Unsubscribe<SpaceShowSignal>(OnSpaceShow);
        _signalBus.Unsubscribe<SpaceOnMinimapRenderSignal>(OnMinimapRender);
        OnDestroyAllAction -= OnDestroyAll;
        if (mapSpaceUi)
        {
            mapSpaceUi.gameObject.SetActive(false);
            mapSpaceUi.Destroy();
        }
        GameObject.Destroy(gameObject);
    }
    void OnSpaceShow(SpaceShowSignal signal)
    {

    }
    void Update()
    {

    }
}