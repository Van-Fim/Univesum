using System.Collections.Generic;
using UnityEngine;

// Основной контроллер на корабле
public class SpAIExecutor : MonoBehaviour
{
    private Ship ship;
    private AICommand currentActiveCommand;

    void Start() => ship = GetComponent<Ship>();

    public void IssueCommand(AICommand newCommand, Dictionary<string, float> mainParams = null)
    {
        if (mainParams != null)
            newCommand.mainParams = mainParams;
        currentActiveCommand = newCommand;
    }

    public void Tick()
    {
        if (currentActiveCommand != null)
        {
            currentActiveCommand.UpdateCommand();
            if (currentActiveCommand.IsCompleted)
                currentActiveCommand = null;
        }
    }
}
