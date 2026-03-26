using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public TextMeshProUGUI currentSpeed;
    public UnityEngine.UI.Image hud01;
    public UnityEngine.UI.Image hull;
    public UnityEngine.UI.Image hud02;
    public UnityEngine.UI.Image shield;
    public UnityEngine.UI.Image hud03;
    public UnityEngine.UI.Image power;
    public UnityEngine.UI.Image crosshair;

    public void Start()
    {
        UnityEngine.UI.Image img01 = hud01.GetComponent<UnityEngine.UI.Image>();
    }
}
