using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerShipController : SpaceObjectController
{
    public override void Start()
    {
        sp_object.canvasController.crosshair.sprite = Resources.Load<Sprite>("Textures/UI/center_crosshair01");
        _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    public override void FixedUpdate()
    {
        if (_rigidbody == null)
        {
            return;
        }

        Turn();
        Move();
    }

    public override void Update()
    {
        if (_rigidbody == null)
        {
            return;
        }
        if (Input.GetKey(KeyCode.X))
        {
            sp_object.cameraManager.GetMainCamera().transform.localPosition = new Vector3(0, 1, -20);
        }
        else
        {
            sp_object.cameraManager.GetMainCamera().transform.localPosition = Vector3.zero;
        }

        if (Input.GetMouseButton(0))
        {
            FireWeapon();
        }
    }

    private void FireWeapon()
    {
        _signalBus.Fire(new WeaponFiredSignal());
    }

    #region Rotation Logic
    public override void Turn()
    {
        Ship ship = (Ship)sp_object;
        int _rotationSpeed = ship.engine.rotationSpeed;
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
    #endregion

    #region Movement Logic
    public override void Move()
    {
        Ship ship = (Ship)sp_object;
        int _accelerationSpeed = ship.engine.accelerationSpeed;
        int _maxSpeed = ship.engine.maxSpeed;
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
        sp_object.canvasController.currentSpeed.text = $"{Mathf.Round(_rigidbody.linearVelocity.magnitude)}/{_maxSpeed}";
        sp_object.signalBus.Fire(new PlayerSpeedChangedSignal(_currentSpeedFactor));
    }
    #endregion
}