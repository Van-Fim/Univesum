using UnityEngine;
using Zenject;

public class ObjectSelector : MonoBehaviour
{
    ISelectable currentSelection;
    [Inject] Player player;
    [Inject] CameraManager cameraManager;
    [Inject] CursorManager cursorManager;
    bool hoveredObject;
    void Update()
    {
        Ray ray = cameraManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);
        bool h = Physics.Raycast(ray, out RaycastHit hit, 30000f);
        if (h)
        {
            var selectable = hit.collider.GetComponent<ISelectable>();
            if (selectable != null && !hoveredObject)
            {
                hoveredObject = true;
                cursorManager.SwitchCursor("Hover");
            }
        }
        else
        {
            if (hoveredObject)
            {
                hoveredObject = false;
                cursorManager.SwitchCursor("Default");
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            if (h)
            {
                var selectable = hit.collider.GetComponent<ISelectable>();
                if (selectable != null)
                {
                    Select(selectable);
                }
                else
                {
                    Deselect();
                }
            }
        }
    }

    void Select(ISelectable target)
    {
        if (currentSelection != null)
            currentSelection.OnDeselect();

        currentSelection = target;
        currentSelection.OnSelect();
    }

    void Deselect()
    {
        if (currentSelection != null)
        {
            currentSelection.OnDeselect();
            currentSelection = null;
        }
    }
}

