using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CanvasController : MonoBehaviour
{
    public GameObject main;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI currentSpeed;
    public UnityEngine.UI.Image hud01;
    public UnityEngine.UI.Image hull;
    public UnityEngine.UI.Image hud02;
    public UnityEngine.UI.Image shield;
    public UnityEngine.UI.Image hud03;
    public UnityEngine.UI.Image power;
    public UnityEngine.UI.Image crosshair;
    public bool is_uiHidden;
    public TargetSelect targetSelect;
    [Inject] private readonly CameraManager cameraManager;
    [Inject] private readonly SignalBus _signalBus;
    public MainMenu mainMenu;

    public static CanvasController singleton;
    void Start()
    {
        singleton = this;
    }
    public void HideUi()
    {
        main.gameObject.SetActive(false);
        targetSelect.gameObject.SetActive(false);
        is_uiHidden = true;
    }
    public void ShowUi()
    {
        main.gameObject.SetActive(true);
        targetSelect.gameObject.SetActive(true);
        is_uiHidden = false;
    }
    public void Init()
    {
        UnityEngine.UI.Image img01 = hud01.GetComponent<UnityEngine.UI.Image>();
        AsteroidSelect pr = Resources.Load<AsteroidSelect>("Prefabs/AsteroidSelect");
        targetSelect = Instantiate(pr);
        targetSelect.transform.SetParent(transform);
        targetSelect.cameraManager = cameraManager;
        targetSelect.canvasController = this;
        targetSelect._signalBus = _signalBus;
    }
}
