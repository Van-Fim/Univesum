using System;
using System.Collections.Generic;
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

    public string GetText(SpaceObject spaceObject)
    {
        string ret = null;
        LangData langData = JsonConfigLoader.LoadFromFile<LangData>($"Lang/{currentLang}/{spaceObject.GetType().FullName}");
        LangDataItem item = langData.data.Find(x => x.codename == spaceObject.spaceObjectConfig.name);
        if (item == null)
        {
            return ret;
        }
        ret = item.text;
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
        ret = item.text;
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