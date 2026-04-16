using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class SaveData
{
    public List<SpaceObjectData> spaceObjectDatas = new List<SpaceObjectData>();
    public List<ShipData> shipDatas = new List<ShipData>();
    public List<StationData> stationDatas = new List<StationData>();
}