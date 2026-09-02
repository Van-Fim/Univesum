using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class ProjectilePool : MonoMemoryPool<Weapon, ProjectileConfig, Vector3, Vector3, Quaternion, Projectile>
{
    protected override void Reinitialize(Weapon weapon, ProjectileConfig config, Vector3 pos, Vector3 targetPos, Quaternion rot, Projectile item)
    {
        item.weapon = weapon;
        item._parent = weapon._parent;
        item.Init();
        item.transform.position = pos;
        item.transform.rotation = rot;

        Vector3 baseDir = (targetPos - weapon.firePointTransform.position).normalized;
        Collider col = item.GetComponent<Collider>();
        Physics.IgnoreCollision(col, weapon._parent.meshCollider);
        if (item.spaceObjectCollider != null && item.spaceObjectCollider != weapon._parent.meshCollider)
        {
            Physics.IgnoreCollision(col, item.spaceObjectCollider, false);
        }
        item.spaceObjectCollider = weapon._parent.meshCollider;
        // Collider col = weapon._parent.meshCollider;
        
        // if (!Projectile.ignoreList.Contains(col))
        // {
        //     Projectile.ignoreList.Add(col);
        //     for (int i = 0; i < Projectile.ignoreList.Count; i++)
        //     {
        //         if (Projectile.ignoreList[i] == null)
        //         {
        //             Projectile.ignoreList.Remove(Projectile.ignoreList[i]);
        //         }
        //     }
        // }

        item.gameObject.SetActive(true);

        item.Launch(config, baseDir, rot);
    }
}
