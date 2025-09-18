using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerController : MonoBehaviour
{
    public CanvasController canvasController;
    public CameraManager cameraManager;
    // Настройки движения
    [Header("Movement Settings")]
    [SerializeField] public int _maxSpeed = 1000000;
    [SerializeField] public int _rotationSpeed = 150;
    [SerializeField] public float _accelerationSpeed = 10000f;

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

    // Кеширование экранных размеров
    public Vector2 _screenCenter;

    [Inject]
    public void Construct(CanvasController canvasController, CameraManager cameraManager)
    {
        this.canvasController = canvasController;
        this.cameraManager = cameraManager;
    }

    public virtual void Start()
    {
        this.canvasController.crosshair.sprite = Resources.Load<Sprite>("Textures/UI/center_crosshair01");
        _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Transform mainTransform = transform.Find("MAIN");
        Transform cameraTransform = mainTransform.Find("CAMERA");
        Camera cam = cameraManager.GetMainCamera();
        cam.transform.SetParent(cameraTransform);
        cam.transform.localPosition = Vector3.zero;
    }

    public virtual void FixedUpdate()
    {
        if (_rigidbody == null)
        {
            return;
        }

        Turn();
        Move();
    }

    #region Rotation Logic
    public virtual void Turn()
    {
        if (!Input.GetMouseButton(1))
        {
            // Управление клавиатурой
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            _rigidbody.transform.Rotate(
                Vector3.up * horizontalInput * KeyboardRotationSpeed,
                Space.World
            );
            _rigidbody.transform.Rotate(
                Vector3.right * verticalInput * KeyboardRotationSpeed,
                Space.Self
            );
        }
        else
        {
            // Управление мышью
            float speed = _rotationSpeed;
            float rollInput = Input.GetAxis("Roll");
            Vector3 mousePosition = Input.mousePosition;

            // Расчет отклонений мыши от центра экрана
            float pitch = (mousePosition.y - _screenCenter.y) / _screenCenter.y;
            float yaw = (mousePosition.x - _screenCenter.x) / _screenCenter.x;

            // Применение чувствительности
            pitch *= MouseSensitivityMultiplier;
            yaw *= MouseSensitivityMultiplier;

            // Ограничение значений
            pitch = -Mathf.Clamp(pitch, -1.0f, 1.0f);
            yaw = Mathf.Clamp(yaw, -1.0f, 1.0f);

            // Устранение "мертвой зоны"
            pitch = ApplyDeadZone(pitch);
            yaw = ApplyDeadZone(yaw);

            // Расчет вращения по крену (roll)
            float roll = speed * Time.deltaTime * rollInput;

            // Применение вращения
            Vector3 rotationAngles = new Vector3(
                pitch * (speed / RotationSpeedDivisor),
                yaw * (speed / RotationSpeedDivisor),
                roll
            );
            _rigidbody.transform.Rotate(rotationAngles);
        }
    }

    /// <summary>
    /// Применяет "мертвую зону" к значению ввода.
    /// </summary>
    public float ApplyDeadZone(float value)
    {
        if (Mathf.Abs(value) <= MouseDeadZone)
            return 0;
        return value;
    }
    #endregion

    #region Movement Logic
    public virtual void Move()
    {
        if (Input.GetKey(KeyCode.Space) && _rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _currentSpeedFactor = _targetSpeedFactor = 0f;
            return;
        }

        float speedChangeFactor = Input.GetAxis("ChangeSpeed");
        _targetSpeedFactor += speedChangeFactor;

        // Ограничение целевого фактора скорости
        _targetSpeedFactor = Mathf.Clamp(
            _targetSpeedFactor,
            MinSpeedFactor,
            MaxSpeedFactor
        );

        // Плавное изменение текущего фактора скорости
        if (_currentSpeedFactor < _targetSpeedFactor)
        {
            _currentSpeedFactor += _accelerationSpeed * Time.fixedDeltaTime;
            if (_currentSpeedFactor > _targetSpeedFactor)
            {
                _currentSpeedFactor = _targetSpeedFactor;
            }
        }
        else if (_currentSpeedFactor > _targetSpeedFactor)
        {
            _currentSpeedFactor -= _accelerationSpeed * Time.fixedDeltaTime;
            if (_currentSpeedFactor < _targetSpeedFactor)
            {
                _currentSpeedFactor = _targetSpeedFactor;
            }
        }

        // Применение силы движения
        _rigidbody.linearVelocity = (transform.forward * _maxSpeed * _currentSpeedFactor);
        canvasController.currentSpeed.text = $"{Mathf.Round(_rigidbody.linearVelocity.magnitude)}/{_maxSpeed}";
    }
    #endregion
}