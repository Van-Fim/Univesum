using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SpaceObjectController : MonoBehaviour
{
    [Inject] public SignalBus _signalBus;
    public SpaceObject sp_object;

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

    public Vector2 _screenCenter;
    public bool IsOwnedByLocalPlayer(Player player)
    {
        return player.GetCurrentController() != null &&
               player.GetCurrentController().sp_object == sp_object;
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