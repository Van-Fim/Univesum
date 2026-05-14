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
    public GameObject mainPanel;
    public List<ButtonData> buttonDatas = new List<ButtonData>();
    UnityAction<byte> OnButtonPressed;
    Button buttonPrefab;
    public void Start()
    {
        buttonPrefab = Resources.Load<Button>("Prefabs/MainMenu/mainMenuButton");
        OnButtonPressed += OnButtonPressedAction;
        CreateButtons();
    }
    public void ClearButtons()
    {
        foreach (Transform child in mainPanel.transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }
    public void CreateButtons()
    {
        ClearButtons();
        for (int i = 0; i < buttonDatas.Count; i++)
        {
            CreateButton(buttonDatas[i]);
        }
    }
    public void CreateButton(ButtonData buttonData)
    {
        byte num = (byte)buttonDatas.IndexOf(buttonData);
        Button newButton = Instantiate<Button>(buttonPrefab, mainPanel.transform);
        newButton.onClick.AddListener(() => OnButtonPressed.Invoke(num));
        newButton.name = $"Button_{buttonData.buttonName}";

        TextMeshProUGUI txt = newButton.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = LangManager.singleton.GetText("UI", buttonData.buttonName);
    }

    public void OnButtonPressedAction(byte v)
    {
        if (v == 0)
        {
            Universe.singleton.Clear();
            var startConfig = JsonConfigLoader.LoadFromFile<GameStartConfig>("Gamestarts/Default");
            GameStartManager.singleton.SetConfig(startConfig);
            GameStartManager.singleton.Load();
            MusicManager.singleton.audioSource.Stop();
            CanvasController.singleton.mainMenu.gameObject.SetActive(false);
            PlayerService.singleton.SetIsInMenu(false);
            CanvasController.singleton.ShowUi();
        }
    }
}
