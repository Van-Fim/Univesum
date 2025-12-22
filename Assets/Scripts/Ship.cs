using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Ship : SpaceObject
{
    public EngineConfig engine;
    public PowerGenerator powerGenerator;
    [Inject] private WeaponFactory _weaponFactory;
    
    public List<Weapon> weapons = new List<Weapon>();

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

    public override void InstallLoadout(Loadout loadout)
    {
        base.InstallLoadout(loadout);

        if (loadout.hardpoints == null)
        {
            return;
        }
        for (int i = 0; i < loadout.hardpoints.Count; i++)
        {
            LoadoutHP hp = loadout.hardpoints[i];
            if (hp.hardpoint == "Engine")
            {
                engine = JsonConfigLoader.LoadFromResources<EngineConfig>("Configs/Engines/" + hp.item);
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
                PowerGeneratorConfig pw = JsonConfigLoader.LoadFromResources<PowerGeneratorConfig>("Configs/PowerGenerators/" + hp.item);
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
                            WeaponConfig cfg = JsonConfigLoader.LoadFromResources<WeaponConfig>("Configs/Weapons/" + hp.item);
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
        }
    }
}
