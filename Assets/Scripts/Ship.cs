using System.Collections.Generic;
using UnityEngine;
using Zenject;
[System.Serializable]
public class ShipData : SpaceObjectData
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
public class Ship : SpaceObject
{
    public override SpaceObjectData Save()
    {
        ShipData shipData = new ShipData();
        shipData.ReadData(this);
        return shipData;
    }
    public override void Fire()
    {
        signalBus.Fire(new WeaponFiredSignal(this));
    }
    public override void InstallConfig()
    {
        base.InstallConfig();
    }
    public override StarSystem SetStarSystem(int galaxyId, int systemId)
    {
        StarSystem oldStarSystem = _StarSystem;
        if (oldStarSystem != null && (oldStarSystem.galaxyId != galaxyId || oldStarSystem.id != systemId) && oldStarSystem.ships.Contains(this))
        {
            oldStarSystem.ships.Remove(this);
            oldStarSystem.allObjs.Remove(this);
        }

        this.galaxyId = galaxyId;
        this.systemId = systemId;
        _StarSystem = GetStarSystem();
        if (_StarSystem != null && !_StarSystem.ships.Contains(this))
        {
            _StarSystem.ships.Add(this);
            _StarSystem.allObjs.Add(this);
            if (is_player)
            {
                Debug.Log($"Player ship {name} added to star system {_StarSystem.name}");
            }
        }
        return _StarSystem;
    }

    public override void BuildLoadouts()
    {
        if(is_player){
            Debug.Log($"Starting building loadouts {loadoutName}");
            Debug.Log($"hardpoints exsists");
            Debug.Log($"loadoutHPs {loadoutHPs.Count}");

            for (int i = 0; i < loadoutHPs.Count; i++)
            {
                LoadoutHP hp = loadoutHPs[i];
            }
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
                            hp.objTransform = JetEngine.transform;
                            hp.upgradeItem = JetEngine;
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
                        if (tr.name == hp.hardpoint && hp.upgradeItem == null)
                        {
                            WeaponConfig cfg = JsonConfigLoader.LoadFromFile<WeaponConfig>("Weapons/" + hp.item);
                            Weapon weapon = _weaponFactory.Create(this, cfg);
                            weapon.Init();
                            weapon.transform.SetParent(tr);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = Quaternion.identity;
                            weapon.InstallConfig();
                            weapons.Add(weapon);
                            hp.objTransform = weapon.transform;
                            hp.upgradeItem = weapon;
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
                        if (tr.name == hp.hardpoint && hp.upgradeItem == null)
                        {
                            WeaponConfig cfg = JsonConfigLoader.LoadFromFile<WeaponConfig>("Weapons/" + hp.item);
                            Weapon turret = _weaponFactory.Create(this, cfg);
                            turret.Init();
                            turret.transform.SetParent(tr);
                            turret.transform.localPosition = Vector3.zero;
                            turret.transform.localRotation = Quaternion.identity;
                            turret.InstallConfig();
                            weapons.Add(turret);
                            hp.objTransform = turret.transform;
                            hp.upgradeItem = turret;
                        }
                    }
                }

            }
            loadoutHPs[i] = hp;
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
    public override void OnTick()
    {
        base.OnTick();
    }
    public override void Update()
    {
        if (IsOwnedByLocalPlayer())
        {
            float dt = Time.deltaTime;
            if (powerGenerator != null)
            {
                powerGenerator.Update(dt);

                float dv = ((float)powerGenerator.currentEnergy / (float)powerGenerator.config.maxEnergy);
                canvasController.power.fillAmount = 0.4f * dv;
            }
        }
    }
}
