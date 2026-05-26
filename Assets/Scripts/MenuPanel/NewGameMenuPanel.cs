using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
public class NewGameMenuPanel : MenuPanel
{
    UnityAction<int> OnButtonPressed;
    Button itemButtonPrefab;
    public TXMText infoText;
    int selectedButtonId;
    public void Start()
    {
        OnButtonPressed += OnButtonPressedAction;
    }
    public void ClearButtons()
    {
        while (itemsParent.childCount > 0)
        {
            foreach (Transform child in itemsParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    public void OnButtonPressedAction(int v)
    {
        selectedButtonId = v;

        string text = "<align=center>{Gamestart:Start" + v + "}</align><br>" + "<align=left>{Gamestart:Start" + v + "Descr}</align>";
        infoText.TextCode = text;
    }
    public void OnButtonPressedOkAction()
    {
        if (selectedButtonId == 0)
        {
            return;
        }
        string zs = null;
        if (selectedButtonId < 10)
        zs = "0";
        GameStartConfig startConfig = JsonConfigLoader.LoadFromFile<GameStartConfig>($"Gamestarts/Start{zs}{selectedButtonId}");
        
        Universe.singleton.Clear();
        GameStartManager.singleton.SetConfig(startConfig);
        GameStartManager.singleton.Load();

        MusicManager.singleton.audioSource.Stop();
        CanvasController.singleton.mainMenu.gameObject.SetActive(false);
        PlayerService.singleton.SetIsInMenu(false);
        CanvasController.singleton.ShowUi();
    }
    public void CreateButton(int id)
    {
        if (!itemButtonPrefab)
        {
            itemButtonPrefab = Resources.Load<Button>("Prefabs/MainMenu/itemButton");
        }
        Button newButton = Instantiate<Button>(itemButtonPrefab, itemsParent.transform);
        newButton.onClick.AddListener(() => OnButtonPressed.Invoke(id));
        newButton.name = $"NewGameButton_{id}";

        TXMText txt = newButton.GetComponentInChildren<TXMText>();
        txt.text = "{" + "Gamestart:Start" + $"{id}" + "}";
    }
}
