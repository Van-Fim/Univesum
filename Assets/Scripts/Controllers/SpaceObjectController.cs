using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;

public class SpaceObjectController : MonoBehaviour
{
    [Inject] public SignalBus _signalBus;
    [Inject] public PlayerService _playerService;
    [Inject] public Universe _universe;
    [Inject] public CanvasController _canvasController;
    [Inject] public SaveManager _saveManager;
    private SpaceObject sp_object;

    // Текущие параметры скорости
    public float _targetSpeedFactor = 0f;
    public float _currentSpeedFactor = 0f;

    // Компоненты
    [SerializeField] public Rigidbody _rigidbody;

    // Константы для управления мышью
    public const float MouseDeadZone = 0.08f;
    public const float MouseSensitivityMultiplier = 3f;
    public const float RotationSpeedDivisor = 100f;
    public const float KeyboardRotationSpeed = 5f;
    public const float MinSpeedFactor = -0.25f;
    public const float MaxSpeedFactor = 1f;

    [Inject] public CameraManager _cameraManager;

    public Vector2 _screenCenter;

    public string mainCommand;
    public string command;
    public string prevCommand;
    public List<string> mainParameters;
    public List<string> parameters;
    public List<string> prevParameters;

    public SpaceObjectController()
    {
        mainParameters = new List<string>();
    }

    public void SetMainCommand(string command, List<string> args = null)
    {
        this.mainCommand = command;
        if (args != null) mainParameters = new List<string>(args);
    }

    public void SetCommand(string command, List<string> args = null)
    {
        this.command = command;
        if (args != null) parameters = new List<string>(args);
    }

    public SpaceObject Sp_object
    {
        get
        {
            return sp_object;
        }
        set
        {
            sp_object = value;
        }
    }


    public bool IsOwnedByLocalPlayer(Player player)
    {
        return player.GetCurrentController() != null &&
               player.GetCurrentController().Sp_object == Sp_object;
    }
    public virtual void Start()
    {

    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void Warp(StarSystem starSystem, Vector3 position, Vector3 rotation)
    {
        if (Sp_object == null || SpaceContainer.singleton == null) return;
        Sp_object.SetStarSystem(starSystem.galaxyId, starSystem.id);
        Sp_object.transform.position = position;
        Sp_object.transform.eulerAngles = rotation;

        WorldChunkManager wcm = WorldChunkManager.singleton;
        if (wcm)
        {
            wcm.Init();
            wcm.isFirstChunksReady = false;
            wcm.UpdateCurrentChunk();
            wcm.UpdateChunksAround(wcm.currentChunk);
            wcm.isFirstChunksReady = true;
        }
        SpaceContainer.singleton.transform.localPosition = Vector3.zero;
        TargetSelect.currentSelectedItem = null;
        TargetSelect.InvokeSelect();
        _signalBus.Fire(new SignalOnPlayerChangedSystem(Sp_object, starSystem));
        SpaceObject.InvokeWarp(Sp_object);
    }
    public virtual void TurnDir(Vector3 direction)
    {

    }
    public virtual void Turn(Vector3 position)
    {

    }
    public virtual void Turn(Transform target = null)
    {

    }

    public float ApplyDeadZone(float value)
    {
        if (Mathf.Abs(value) <= MouseDeadZone)
            return 0;
        return value;
    }
    public virtual void Move(Vector3 position)
    {

    }
    public virtual void Move(float spfc = -1f, Transform target = null)
    {

    }
}
