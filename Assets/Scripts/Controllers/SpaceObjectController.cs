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
    public List<string> parameters;

    public SpaceObjectController()
    {
        parameters = new List<string>();
    }
    
    public void SetCommand(string command, params string[] args)
    {
        mainCommand = command;
        parameters = new List<string>(args);
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

    }

    public virtual void Turn()
    {

    }

    public float ApplyDeadZone(float value)
    {
        if (Mathf.Abs(value) <= MouseDeadZone)
            return 0;
        return value;
    }

    public virtual void Move()
    {

    }
}