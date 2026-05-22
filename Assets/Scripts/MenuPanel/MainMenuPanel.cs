using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
public class MainMenuPanel : MenuPanel
{
    public List<ButtonData> buttonDatas = new List<ButtonData>();
    UnityAction<int> OnButtonPressed;
    Button mainMenuButtonPrefab;
    public void Start()
    {
        mainMenuButtonPrefab = Resources.Load<Button>("Prefabs/MainMenu/mainMenuButton");
        OnButtonPressed += OnButtonPressedAction;
        CreateButtons();
    }
    public void ClearButtons()
    {
        foreach (Transform child in transform)
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
   
        Button newButton = Instantiate<Button>(mainMenuButtonPrefab, transform);
        newButton.onClick.AddListener(() => OnButtonPressed.Invoke(num));
        newButton.name = $"Button_{buttonData.buttonName}";

        TXMText txt = newButton.GetComponentInChildren<TXMText>();
        txt.text = "{" + buttonData.buttonName + "}";
    }
    
    public void OnButtonPressedAction(int v)
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
        else if (v == 1)
        {
            gameObject.SetActive(false);
            LoadMenuPanel loadMenuPanel = mainMenu.loadGames;
            loadMenuPanel.gameObject.SetActive(true);
            mainMenu.prev = loadMenuPanel;
            List<string> list = new List<string>();
            list = SaveManager.singleton.GetAllSaves();
            loadMenuPanel.ClearSaveButtons();
            for (int i = 0; i < list.Count; i++)
            {
                loadMenuPanel.CreateSaveButton((i + 1));
            }
        }
        else if (v == 2)
        {
            gameObject.SetActive(false);
            mainMenu.saveGame.gameObject.SetActive(true);
            mainMenu.prev = mainMenu.saveGame;
        }
        else if (v == 3)
        {
            gameObject.SetActive(false);
            mainMenu.settings.gameObject.SetActive(true);
            mainMenu.prev = mainMenu.settings;
        }
    }
}
