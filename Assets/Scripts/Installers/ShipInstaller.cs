using UnityEngine;
using Zenject;
public class ShipInstaller : MonoInstaller
{
    public GameObject defaultWeapon;
    public GameObject defaultTurret;

    public override void InstallBindings()
    {
        Container.BindFactory<SpaceObject, WeaponConfig, Weapon, WeaponFactory>()
                 .FromComponentInNewPrefab(defaultWeapon);
    }
}