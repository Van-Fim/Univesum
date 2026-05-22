using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
[Serializable]
public class LangData
{
    public string name;
    public List<LangDataItem> data = new List<LangDataItem>();
}
[Serializable]
public class LangDataItem
{
    public string codename;
    public string text;

}
public class LangManager
{
    public static string currentLang = "RU";
    [Inject] CanvasController _canvasController;
    [Inject] SignalBus _signalBus;
    public static LangManager singleton;
    public void Init()
    {
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);
        singleton = this;
    }
    public string ProcessAndReplace(string input, Func<string, string> processor)
    {
        return Regex.Replace(input, @"\{([^}]*)\}", match =>
        {
            string code = match.Groups[1].Value; // Получаем код без {}

            string[] parts = code.Split(':');
            if (parts.Length > 1)
            {
                code = GetText(parts[0], parts[1]);
                code = ProcessAndReplace(code, cd =>
                {
                    return $"{cd}";
                });
            }

            // Обрабатываем и возвращаем замену
            return processor(code);
        });
    }

    public string GetText(SpaceObject spaceObject)
    {
        string ret = null;
        LangData langData = JsonConfigLoader.LoadFromFile<LangData>($"Lang/{currentLang}/{spaceObject.GetType().FullName}");
        LangDataItem item = langData.data.Find(x => x.codename == spaceObject.spaceObjectConfig.name);
        if (item == null)
        {
            return ret;
        }
        ret = ProcessAndReplace(item.text, code =>
        {
            return $"{code}";
        });
        return ret;
    }
    public string GetText(string categoryName, string codeName)
    {
        string ret = null;
        LangData langData = JsonConfigLoader.LoadFromFile<LangData>($"Lang/{currentLang}/{categoryName}");
        if (langData == null)
        {
            return $"Err[{categoryName}:{codeName}]";
        }
        LangDataItem item = langData.data.Find(x => x.codename == codeName);
        if (item == null)
        {
            return $"Err[{categoryName}:{codeName}]";
        }
        ret = ProcessAndReplace(item.text, code =>
        {
            return $"{code}";
        });
        return ret;
    }
    public void OnUpdateTick()
    {
        if (TargetSelect.currentSelectedItem && TargetSelect.currentSelectedItem.spaceObject)
        {
            SpaceObject spaceObject = TargetSelect.currentSelectedItem.spaceObject;
            _canvasController.infoName.text = GetText(spaceObject);
        }
        else
        {
            _canvasController.infoName.text = null;
        }
    }
}