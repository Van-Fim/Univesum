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
}
public class Station : SpaceObject, ISelectable
{
    [Inject] private TurretFactory _turretFactory;

    public List<Turret> turrets = new List<Turret>();
    public override SpaceObjectData Save()
    {
        StationData stationData = new StationData();
        stationData.ReadData(this);
        return stationData;
    }

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
