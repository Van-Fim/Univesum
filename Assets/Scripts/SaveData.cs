using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class SaveData
{
    public int seed;
    public int playerGalaxyId;
    public int playerSystemId;
    public string dateTime;
    public Vector3Int currentChunkPos = new Vector3Int();
    public Vector3 worldPos = new Vector3();

    public Vector3 spContainerPosition = new Vector3();
    public Vector3 spContainerRotation = new Vector3();

    public List<SpaceObjectData> spaceObjectDatas = new List<SpaceObjectData>();
    public List<ShipData> shipDatas = new List<ShipData>();
    public List<StationData> stationDatas = new List<StationData>();
    public List<SpaceConfig> spaceConfigs = new List<SpaceConfig>();
}