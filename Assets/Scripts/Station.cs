using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class StationData : SpaceObjectData
{
    public override bool ReadData(SpaceObject spaceObject)
    {
        bool ret = false;
        if (!spaceObject)
        {
            return ret;
        }
        bool defret = base.ReadData(spaceObject);
        ret = true && defret;
        return ret;
    }
    public override bool InstallData(SpaceObject spaceObject)
    {
        bool ret = false;
        if (!spaceObject)
        {
            return ret;
        }
        bool defret = base.InstallData(spaceObject);
        ret = true && defret;
        return ret;
    }
}
public class Station : SpaceObject, ISelectable
{
    public override SpaceObjectData Save()
    {
        StationData stationData = new StationData();
        stationData.ReadData(this);
        return stationData;
    }

    public override SpaceObject LoadData(SpaceObjectData spaceObjectData)
    {
        Station station = _spaceObjectFactory.Create<Station>(
            "Prefabs/StationPrefab",
            "SpaceObjects/Stations/" + spaceObjectData.spaceObjectConfig.name
        );
        return station;
    }
    public override StarSystem SetStarSystem(int galaxyId, int systemId)
    {
        StarSystem oldStarSystem = _StarSystem;
        if (oldStarSystem != null && (oldStarSystem.galaxyId != galaxyId || oldStarSystem.id != systemId) && oldStarSystem.stations.Contains(this))
        {
            oldStarSystem.stations.Remove(this);
        }

        this.galaxyId = galaxyId;
        this.systemId = systemId;
        _StarSystem = GetStarSystem();
        if (_StarSystem != null && !_StarSystem.stations.Contains(this))
        {
            _StarSystem.stations.Add(this);
        }
        return _StarSystem;
    }
    public override void Update()
    {

    }

    public override void BuildLoadouts()
    {
        if (hardpoints == null)
        {
            return;
        }
        for (int i = 0; i < loadoutHPs.Count; i++)
        {
            LoadoutHP hp = loadoutHPs[i];
            if (hp.hardpoint == "Engine")
            {
                engine = JsonConfigLoader.LoadFromFile<EngineConfig>("Engines/" + hp.item);
                if (hardpoints != null && hardpoints.childCount > 0)
                {
                    for (int j = 0; j < hardpoints.childCount; j++)
                    {
                        Transform tr = hardpoints.GetChild(j);
                        if (tr.name.StartsWith("HPFXEngine"))
                        {
                            // TrailRenderer trail = GameObject.Instantiate(Resources.Load<TrailRenderer>("Prefabs/EngineTrail"));
                            // trail.transform.SetParent(tr);
                            // trail.transform.localPosition = Vector3.zero;
                            // trail.transform.localRotation = Quaternion.identity;

                            GameObject JetEngineGameObject = _container.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/JetEngine"));
                            JetEngineController JetEngine = JetEngineGameObject.GetComponent<JetEngineController>();
                            JetEngine._parent = this;
                            JetEngine.Init();
                            JetEngine.transform.SetParent(tr);
                            JetEngine.transform.localPosition = Vector3.zero;
                            JetEngine.transform.localRotation = Quaternion.identity;
                            JetEngine.ApplyGradient(engine.color01, engine.color02);

                            EngineSoundController engineSoundController = JetEngineGameObject.GetComponent<EngineSoundController>();
                            engineSoundController.InstallSounds(engine);
                            engineSoundController.sp_object = this;
                        }
                    }
                }
            }
            else if (hp.hardpoint == "PowerGenerator")
            {
                PowerGeneratorConfig pw = JsonConfigLoader.LoadFromFile<PowerGeneratorConfig>("PowerGenerators/" + hp.item);
                if (hardpoints != null && hardpoints.childCount > 0)
                {
                    powerGenerator = new PowerGenerator(pw);
                }
            }
            else if (hp.hardpoint.StartsWith("HPWeapon"))
            {
                if (hardpoints != null && hardpoints.childCount > 0)
                {
                    for (int j = 0; j < hardpoints.childCount; j++)
                    {
                        Transform tr = hardpoints.GetChild(j);
                        if (tr.name == hp.hardpoint)
                        {
                            WeaponConfig cfg = JsonConfigLoader.LoadFromFile<WeaponConfig>("Weapons/" + hp.item);
                            Weapon weapon = _weaponFactory.Create(this, cfg);
                            weapon.Init();
                            weapon.transform.SetParent(tr);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = Quaternion.identity;
                            weapon.InstallConfig();
                        }
                    }
                }

            }
            else if (hp.hardpoint.StartsWith("HPTurret"))
            {
                if (hardpoints != null && hardpoints.childCount > 0)
                {
                    for (int j = 0; j < hardpoints.childCount; j++)
                    {
                        Transform tr = hardpoints.GetChild(j);
                        if (tr.name == hp.hardpoint)
                        {
                            WeaponConfig cfg = JsonConfigLoader.LoadFromFile<WeaponConfig>("Weapons/" + hp.item);
                            Weapon turret = _weaponFactory.Create(this, cfg);
                            turret.Init();
                            turret.transform.SetParent(tr);
                            turret.transform.localPosition = Vector3.zero;
                            turret.transform.localRotation = Quaternion.identity;
                            turret.InstallConfig();
                        }
                    }
                }

            }
        }
    }

    public void OnSelect()
    {
        if (targetSelect)
        {
            TargetSelect.currentSelectedItem = targetSelect;
            TargetSelect.InvokeSelect();
        }
    }

    public void OnDeselect()
    {
        return;
    }

    public string GetLabel()
    {
        string ret = null;

        canvasController.infoName.text = ret;
        return ret;
    }
}
