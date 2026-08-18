using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;
using UnityEngine.Events;

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

    public static UnityAction OnTickAction;

    public SpAIExecutor aIExecutor;

    public SpaceObjectController()
    {
        mainParameters = new List<string>();
        OnTickAction += OnTick;
    }
    public virtual void Destroy()
    {
        OnTickAction -= OnTick;
    }
    public virtual void OnTick()
    {
        AvoidObstacles();
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
            aIExecutor = value.aIExecutor;
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

    public virtual void AvoidObstacles()
    {
        if (PlayerService.singleton.GetStarSystem() != Sp_object._StarSystem)
        {
            return;
        }
        Vector3 moveDir = Vector3.zero;
        float raycastDistance = 100f;
        Collider[] hits = Physics.OverlapSphere(
                    Sp_object.transform.position,
                    raycastDistance,
                    LayerMask.GetMask("Default", "Sensor")
                );
        List<Collider> detectedObjects = new List<Collider>();
        Collider dsp = null;
        SpaceObject dspSp = null;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject != sp_object.gameObject && DirectionHelper.IsInFront(sp_object.transform, hits[i].transform, 10f))
            {
                detectedObjects.Add(hits[i]);
                dsp = hits[i];
                dspSp = dsp.gameObject.GetComponent<SpaceObject>();
            }
        }
        if (detectedObjects.Count > 0 && sp_object.aIExecutor != null)
        {
            Vector3 obstaclePos = dsp.transform.position;
            Vector3 directionToObstacle = (obstaclePos - sp_object.transform.position).normalized;

            // Используем локальную ось X или Y корабля для создания вектора облета
            // Это заставит корабль уходить в сторону относительно своего курса
            int rn = Random.Range(0, 2);
            if (rn == 1)
            {
                moveDir = sp_object.transform.right;
            }
            else
            {
                moveDir = sp_object.transform.up;
            }
            Vector3 sideStep = moveDir * (Random.value > 0.5f ? 1 : -1);

            AIEvadingEvent aIEvadingEvent = new AIEvadingEvent();
            aIEvadingEvent.targetScale = 200;
            Vector3 ppp = (directionToObstacle + sideStep).normalized;
            if (dspSp)
            {
                aIEvadingEvent.targetScale = dspSp.scaleValue;
            }
            int sc = sp_object.scaleValue + aIEvadingEvent.targetScale;
            aIEvadingEvent.evadingDirection = ppp;
            aIEvadingEvent.evadingPosition = obstaclePos + aIEvadingEvent.evadingDirection * (sc*30);
            aIEvadingEvent.spaceObjectId = sp_object.id;

            if (dspSp)
            {
                if(dspSp.debugSphere){
                    dspSp.debugSphere.transform.localPosition = aIEvadingEvent.evadingPosition;
                }
            }

            AICommand.InvokeInterrupt(aIEvadingEvent);
        }
    }

    public static void InvokeTick()
    {
        OnTickAction?.Invoke();
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
