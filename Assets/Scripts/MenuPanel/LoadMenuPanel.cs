using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
public class LoadMenuPanel : MenuPanel
{
    UnityAction<int> OnSaveButtonPressed;
    Button saveItemButtonPrefab;
    public TXMText infoText;
    int selectedButtonId;
    public void Start()
    {
        OnSaveButtonPressed += OnSaveButtonPressedAction;
    }
    public void ClearSaveButtons()
    {
        while (itemsParent.childCount > 0)
        {
            foreach (Transform child in itemsParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    public void OnSaveButtonPressedAction(int v)
    {
        selectedButtonId = v;
        SaveData save = SaveManager.singleton.GetSaveData(v);
        string text = "<align=center>{UI:SaveName} " + $"{v}</align><br>" + "<align=left>{UI:SaveWhere}: " + $"System_{save.playerGalaxyId}_{save.playerSystemId}</align><br>" + "<align=left>{UI:SaveDateTime}: " + $"{save.dateTime}</align>";
        infoText.TextCode = text;
        // SaveManager.singleton.LoadGame(v);
        // MusicManager.singleton.audioSource.Stop();
        // CanvasController.singleton.mainMenu.gameObject.SetActive(false);
        // PlayerService.singleton.SetIsInMenu(false);
        // CanvasController.singleton.ShowUi();
    }
    public void OnSaveButtonPressedOkAction()
    {
        if (selectedButtonId == 0)
        {
            return;
        }
        SaveManager.singleton.LoadGame(selectedButtonId);
        MusicManager.singleton.audioSource.Stop();
        CanvasController.singleton.mainMenu.gameObject.SetActive(false);
        PlayerService.singleton.SetIsInMenu(false);
        CanvasController.singleton.ShowUi();
    }
    public void CreateSaveButton(int id)
    {
        if (!saveItemButtonPrefab)
        {
            saveItemButtonPrefab = Resources.Load<Button>("Prefabs/MainMenu/itemButton");
        }
        Button newButton = Instantiate<Button>(saveItemButtonPrefab, itemsParent.transform);
        newButton.onClick.AddListener(() => OnSaveButtonPressed.Invoke(id));
        newButton.name = $"SaveButton_{id}";

        TXMText txt = newButton.GetComponentInChildren<TXMText>();
        txt.text = "{" + "UI:SaveName" + "}" + $" {id}";
    }
}
