using System.Collections.Generic;
using UnityEngine;

// Основной контроллер на корабле
public class SpAIExecutor : MonoBehaviour
{
    public Ship ship;
    private AICommand currentActiveCommand;

    public AICommand CurrentActiveCommand
    {
        get => currentActiveCommand; set
        {
            currentActiveCommand = value;
        }
    }

    public void IssueCommand(AICommand newCommand, Dictionary<string, float> mainParams = null)
    {
        if (mainParams != null)
            newCommand.mainParams = mainParams;

        CurrentActiveCommand = newCommand;
    }

    public void Tick()
    {
        if (CurrentActiveCommand != null)
        {
            CurrentActiveCommand.UpdateCommand();
            if (CurrentActiveCommand.IsCompleted)
                CurrentActiveCommand = null;
        }
    }
}
