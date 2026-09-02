using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerShipController : SpaceObjectController
{
    public override void Start()
    {
        Sp_object.canvasController.crosshair.sprite = Resources.Load<Sprite>("Textures/UI/center_crosshair01");
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
    public override void OnTick()
    {
        if (Sp_object == null)
        {
            return;
        }
        float dv = (float)Sp_object.hull/(float)Sp_object.maxHull;
        float dv1 = (float)Sp_object.shield/(float)Sp_object.maxShield;

        Sp_object.canvasController.hull.fillAmount = dv;
        Sp_object.canvasController.shield.fillAmount = dv1;
        base.OnTick();
    }
    public override void Update()
    {
        if (_playerService.IsInMenu())
        {
            return;
        }
        if (_rigidbody == null)
        {
            return;
        }
        if (Input.GetKey(KeyCode.X))
        {
            Sp_object.cameraManager.GetMainCamera().transform.localPosition = new Vector3(0, 1, -20);
        }
        else
        {
            Sp_object.cameraManager.GetMainCamera().transform.localPosition = Vector3.zero;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (MapSpaceUi.currentSelectedItem == null)
            {
                return;
            }
            if (MapSpaceUi.currentSelectedItem.space == null)
            {
                return;
            }

            Camera mainCam = _cameraManager.GetMainCamera();
            Camera mapCam = _cameraManager.GetMapCamera();
            StarSystem psys = _playerService.GetStarSystem();
            int rnd = Random.Range(0, _universe.systemsList.Count);
            StarSystem fsys = (StarSystem)MapSpaceUi.currentSelectedItem.space;
            _playerService.Warp(fsys, Vector3.zero, Vector3.zero);

            psys = _playerService.GetStarSystem();
            mapCam.transform.localPosition = psys.transform.localPosition + new Vector3(0, 200, 0);
            mapCam.transform.localEulerAngles = new Vector3(90, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            CameraManager.InvokeMapSwitch(0);
            Camera mainCam = _cameraManager.GetMainCamera();
            Camera mapCam = _cameraManager.GetMapCamera();
            StarSystem psys = _playerService.GetStarSystem();
            bool st1 = mainCam.enabled;

            mainCam.enabled = !st1;
            mapCam.enabled = st1;
            if (st1)
            {
                _canvasController.HideUi();
            }
            else
            {
                _canvasController.ShowUi();
            }
            mapCam.transform.localPosition = psys.transform.localPosition + new Vector3(0, 200, 0);
            mapCam.transform.localEulerAngles = new Vector3(90, 0, 0);

            PSpace.InvokeMinimapRender(typeof(StarSystem));
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            CameraManager.InvokeMapSwitch(1);
            Camera mainCam = _cameraManager.GetMainCamera();
            Camera mapCam = _cameraManager.GetMapCamera();
            StarSystem psys = _playerService.GetStarSystem();
            bool st1 = mainCam.enabled;

            mainCam.enabled = !st1;
            mapCam.enabled = st1;
            if (st1)
            {
                _canvasController.HideUi();
            }
            else
            {
                _canvasController.ShowUi();
            }
            mapCam.transform.localPosition = psys.transform.localPosition + new Vector3(0, 200, 0);
            mapCam.transform.localEulerAngles = new Vector3(90, 0, 0);

            PSpace.InvokeMinimapRender(typeof(StarSystem));
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _saveManager.SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            _saveManager.LoadGame();
        }
        if (Input.GetMouseButton(0))
        {
            FireWeapon();
        }
    }

    private void FireWeapon()
    {
        _signalBus.Fire(new WeaponFiredSignal(Sp_object));
    }

    #region Rotation Logic
    public override void Turn(Transform target = null)
    {
        if (_cameraManager.GetMapCamera().enabled)
        {
            return;
        }
        Ship ship = (Ship)Sp_object;
        if (ship.engine == null)
        {
            return;
        }
        int _rotationSpeed = ship.engine.rotationSpeed;
        Ray ray = Sp_object.cameraManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);

        if (!Input.GetMouseButton(1))
        {
            // Управление клавиатурой
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            _rigidbody.transform.Rotate(
                Vector3.up * horizontalInput * KeyboardRotationSpeed,
                UnityEngine.Space.World
            );
            _rigidbody.transform.Rotate(
                Vector3.right * verticalInput * KeyboardRotationSpeed,
                UnityEngine.Space.Self
            );
        }
        else
        {
            if (_cameraManager.GetMapCamera().enabled)
            {
                return;
            }
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
    public override void Move(float spfc = -1f, Transform target = null)
    {
        Ship ship = (Ship)Sp_object;
        if (ship.engine == null)
        {
            return;
        }
        int _accelerationSpeed = ship.engine.accelerationSpeed;
        int _maxSpeed = ship.engine.maxSpeed;
        if (Input.GetKey(KeyCode.Space) && _rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _currentSpeedFactor = _targetSpeedFactor = 0f;
            return;
        }
        float speedChangeFactor = Input.GetAxis("ChangeSpeed");
        if (spfc != -1f)
        {
            _targetSpeedFactor = spfc;
        }
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
        Sp_object.canvasController.currentSpeed.text = $"{Mathf.Round(_rigidbody.linearVelocity.magnitude)}/{_maxSpeed}";
        Sp_object.signalBus.Fire(new PlayerSpeedChangedSignal(_currentSpeedFactor));
    }
    #endregion
}
