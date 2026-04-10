using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Station : SpaceObject
{
    [Inject] private TurretFactory _turretFactory;
    
    public List<Turret> turrets = new List<Turret>();

    public override void Update()
    {

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
                            Turret turret = _turretFactory.Create(this, cfg);
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
}
