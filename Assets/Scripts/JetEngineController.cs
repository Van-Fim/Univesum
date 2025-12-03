using UnityEngine;
using UnityEngine.VFX;
using Zenject;

public class JetEngineController : MonoBehaviour
{
    [Inject]
    SignalBus _signalBus;
    [Inject]
    Player player;

    public VisualEffect effect;
    public Gradient gradient;
    public SpaceObject sp_object;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<PlayerSpeedChangedSignal>(OnPlayerSpeedChanged);
    }

    private void OnDestroy()
    {
        _signalBus.Unsubscribe<PlayerSpeedChangedSignal>(OnPlayerSpeedChanged);
    }

    private void OnPlayerSpeedChanged(PlayerSpeedChangedSignal signal)
    {
        if (player.GetCurrentController() != null)
        {
            PlayerController contr = player.GetCurrentController();
            if (contr.sp_object != sp_object)
            {
                return;
            }
        }
        else
        {
            return;
        }
        effect.SetFloat("ConeHeight", Mathf.Abs(signal.SpeedFactor * 0.5f));
        effect.SetInt("Rate", (int)Mathf.Abs(100 + signal.SpeedFactor * 200));
    }
    public void ApplyGradient(Color32 color01, Color32 color02)
    {
        // Создаём новый градиент
        gradient = new Gradient();

        // Цветовые ключи (от синего к красному)
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = color01;
        colorKeys[0].time = 0.502f;   // начало
        colorKeys[1].color = color02;
        colorKeys[1].time = 0.509f;   // конец

        // Альфа-ключи (прозрачность)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0].alpha = 1f;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;
        alphaKeys[1].time = 0.80f;
        alphaKeys[2].alpha = 0f;
        alphaKeys[2].time = 1f;

        // Применяем ключи
        gradient.SetKeys(colorKeys, alphaKeys);

        // Передаём в VFX Graph
        effect.SetGradient("GradColors", gradient);
    }
}
