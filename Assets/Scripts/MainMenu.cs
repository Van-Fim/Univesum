using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
[System.Serializable]
public class ButtonData
{
    public string buttonName;
    public byte btnEvent;
}
public class MainMenu : MonoBehaviour
{
    public MainMenuPanel mainPanel;

    public MenuPanel newGame;
    public LoadMenuPanel loadGames;
    public MenuPanel saveGame;
    public MenuPanel settings;
    public MenuPanel prev;


    public static Color32 btnColor = new Color32(0, 54, 80, 152);
    public void Start()
    {

    }
}
