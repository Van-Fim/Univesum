using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class SaveManager
{
    private string savePath;
    [Inject] private Universe _universe;
    public void Avake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }
    public void SaveGame()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        SaveData saveData = new SaveData();
        for (int i = 0; i < _universe.allSpaceObjects.Count; i++)
        {
            SpaceObject spaceObject = _universe.allSpaceObjects[i];
            SpaceObjectData data = spaceObject.Save();
            if (data is StationData)
            {
                saveData.stationDatas.Add((StationData)data);
            }
            else if (data is ShipData)
            {
                saveData.shipDatas.Add((ShipData)data);
            }
            else
            {
                saveData.spaceObjectDatas.Add(data);
            }
        }
        JsonConfigLoader.SaveToFile<SaveData>(saveData, savePath);
    }
}