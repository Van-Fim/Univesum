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
    [Inject] private NpcJobManager _npcJobManager;
    [Inject] private SpaceObjectFactory _spaceObjectFactory;
    [Inject] private SpaceContainer _spaceContainer;
    [Inject] private readonly DiContainer _container;
    [Inject] private PlayerService _playerService;
    public void Avake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }
    public void SaveGame()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        SaveData saveData = new SaveData();
        saveData.currentChunkPos = WorldChunkManager.singleton.currentChunk;
        saveData.worldPos = WorldChunkManager.singleton.worldPos;
        saveData.spContainerPosition = _spaceContainer.transform.localPosition;
        saveData.spContainerRotation = _spaceContainer.transform.localEulerAngles;
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
    public void LoadGame()
    {
        _npcJobManager.isEnabled = false;
        _npcJobManager.ClearAllJobsAndData();
        _npcJobManager.LoadJobs();
        SpaceObject.InvokeDestroyAll();
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        SaveData saveData = JsonConfigLoader.LoadFromFile<SaveData>(savePath);
        _spaceContainer.transform.localPosition = saveData.spContainerPosition;
        _spaceContainer.transform.localEulerAngles = saveData.spContainerRotation;
        for (int i = 0; i < saveData.stationDatas.Count; i++)
        {
            StationData data = saveData.stationDatas[i];
            Station station = _spaceObjectFactory.Create<Station>(
            "Prefabs/StationPrefab"
            );
            data.InstallData(station);
            Vector3 pos = station.transform.localPosition;
            Vector3 rot = station.transform.localEulerAngles;

            station.Init();
            station.TryInstallConfig();
            station.transform.localPosition = pos;
            station.transform.localEulerAngles = rot;
            station.BuildLoadouts();
            if (station.jobId >= 0)
            {
                _npcJobManager.AddStationForJob(station);
            }
        }
        for (int i = 0; i < saveData.shipDatas.Count; i++)
        {
            ShipData data = saveData.shipDatas[i];
            Ship ship = _spaceObjectFactory.Create<Ship>(
            "Prefabs/ShipPrefab"
            );
            data.InstallData(ship);
            Vector3 pos = ship.transform.localPosition;
            Vector3 rot = ship.transform.localEulerAngles;
            ship.StarSystem = ship.GetStarSystem();

            ship.Init();
            ship.TryInstallConfig();
            if (ship.is_player)
            {
                _playerService.Warp(ship.StarSystem, pos, rot);
                var controller = ship.gameObject.AddComponent<PlayerShipController>();
                _container.Inject(controller);
                controller._rigidbody = ship.rigidbody;
                controller.Sp_object = ship;
                ship.is_player = true;

                ship.spaceObjectController = controller;
                _playerService._player.currentController = controller;
                ship.InstallCamera();
                // WorldChunkManager.singleton.noAsteroids = true;
                SpaceObject.InvokeDestroyAll(typeof(Asteroid));
                WorldChunkManager.singleton.ClearChunks();
                WorldChunkManager.singleton.Init();
                WorldChunkManager.singleton.UpdateCurrentChunk();
                WorldChunkManager.singleton.worldPos = saveData.worldPos;
                WorldChunkManager.singleton.currentChunk = saveData.currentChunkPos;
                _spaceContainer.transform.localPosition = saveData.spContainerPosition;
                _spaceContainer.transform.localEulerAngles = saveData.spContainerRotation;
                WorldChunkManager.singleton.loadedChunks = new Dictionary<Vector3Int, Chunk>();
                WorldChunkManager.singleton.isFirstChunksReady = false;
                WorldChunkManager.singleton.UpdateChunksAround(saveData.currentChunkPos);
                WorldChunkManager.singleton.isFirstChunksReady = true;
            }
            ship.transform.localPosition = pos;
            ship.transform.localEulerAngles = rot;
            ship.BuildLoadouts();
        }
        _npcJobManager.isEnabled = true;
    }
}