using UnityEngine;
using Zenject;

public class AsteroidSelector : MonoBehaviour
{
    ISelectable currentSelection;
    [Inject] CameraManager cameraManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // ЛКМ
        {
            Ray ray = cameraManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 30000f))
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

