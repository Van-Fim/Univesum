using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public TextMeshProUGUI currentSpeed;
    public GameObject hud01;
    public GameObject hud02;
    public UnityEngine.UI.Image crosshair;

    public void Start()
    {
        UnityEngine.UI.Image img01 = hud01.GetComponent<UnityEngine.UI.Image>();
    }
}
