using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class ProjectilePool : MonoMemoryPool<Weapon, ProjectileConfig, Vector3, Vector3, Quaternion, Projectile>
{
    protected override void Reinitialize(Weapon weapon, ProjectileConfig config, Vector3 pos, Vector3 targetPos, Quaternion rot, Projectile item)
    {
        item.weapon = weapon;
        item.Init();
        item.transform.position = pos;
        item.transform.rotation = rot;

        Vector3 baseDir = (targetPos - weapon.baseTransform.position).normalized;

        Physics.IgnoreCollision(item.GetComponent<Collider>(), weapon._parent.meshCollider);

        item.gameObject.SetActive(true);
        item.Launch(config, baseDir, rot);
    }
}
