using UnityEngine;
using Zenject;
public class MenuPanel : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject mainPanel;
    public Transform itemsParent;
    public MainMenu mainMenu;

    public virtual void OnBackPressed()
    {
        mainMenu.mainPanel.gameObject.SetActive(true);
        if (mainMenu.prev)
        {
            mainMenu.prev.gameObject.SetActive(false);
        }
    }
}
