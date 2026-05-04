using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class SaveData
{
    public Vector3Int currentChunkPos = new Vector3Int();
    public Vector3 worldPos = new Vector3();

    public Vector3 spContainerPosition = new Vector3();
    public Vector3 spContainerRotation = new Vector3();

    public List<SpaceObjectData> spaceObjectDatas = new List<SpaceObjectData>();
    public List<ShipData> shipDatas = new List<ShipData>();
    public List<StationData> stationDatas = new List<StationData>();
}