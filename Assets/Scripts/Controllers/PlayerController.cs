using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerController : MonoBehaviour
{
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

    // Кеширование экранных размеров
    public Vector2 _screenCenter;

    public virtual void Start()
    {
        sp_object.canvasController.crosshair.sprite = Resources.Load<Sprite>("Textures/UI/center_crosshair01");
        _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
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

    public virtual void Update()
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

        // float speed = _rigidbody.linearVelocity.magnitude;
        // for (int i = 0; i < sp_object.trails.Count; i++)
        // {
        //     TrailRenderer tr = sp_object.trails[i];
        //     tr.time = Mathf.Lerp(0.010f, 0.025f, speed / 500f); 
        // }
    }

    #region Rotation Logic
    public virtual void Turn()
    {

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

    }
    #endregion
}