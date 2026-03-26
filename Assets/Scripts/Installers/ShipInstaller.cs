using UnityEngine;
using Zenject;
public class ShipInstaller : MonoInstaller
{
    public GameObject defaultWeapon;

    public override void InstallBindings()
    {
        Container.BindFactory<Ship, WeaponConfig, Weapon, WeaponFactory>()
                 .FromComponentInNewPrefab(defaultWeapon);
    }
}