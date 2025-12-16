using UnityEngine;
using Zenject;

public class ProjectilePool : MonoMemoryPool<Weapon, ProjectileConfig, Vector3, Quaternion, Projectile>
{
    protected override void Reinitialize(Weapon weapon, ProjectileConfig config, Vector3 pos, Quaternion rot, Projectile item)
    {
        item.weapon = weapon;
        item.transform.position = pos;
        item.transform.rotation = rot;
        item.gameObject.SetActive(true);
        item.Launch(config, weapon.firePointTransform.forward, rot);
    }
}
