using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class ProjectilePool : MonoMemoryPool<Weapon, ProjectileConfig, Vector3, Vector3, Quaternion, Projectile>
{
    protected override void Reinitialize(Weapon weapon, ProjectileConfig config, Vector3 pos, Vector3 targetPos, Quaternion rot, Projectile item)
    {
        item.weapon = weapon;
        item.transform.position = pos;
        item.transform.rotation = rot;

        Vector3 baseDir = (targetPos - weapon.baseTransform.position).normalized;

        Physics.IgnoreCollision(item.GetComponent<Collider>(), weapon._parent.meshCollider);
        if (item.beam == null)
        {
            item.beam = item.transform.Find("Beam").GetComponent<ParticleSystem>();

            // ParticleSystem.main возвращает struct (MainModule), поэтому меняем через локальную переменную
            var beamMain = item.beam.main;
            beamMain.startColor = new ParticleSystem.MinMaxGradient(config.baseColor);
        }
        if (item.body == null)
        {
            item.body = item.transform.Find("Body").GetComponent<ParticleSystem>();

            var bodyMain = item.body.main;
            bodyMain.startColor = new ParticleSystem.MinMaxGradient(config.baseColor);
        }
        LineRenderer lineRenderer = null;
        if (item.tail == null)
        {
            ProjectileTailLineRenderer prtr = item.AddComponent<ProjectileTailLineRenderer>();
            lineRenderer = item.AddComponent<LineRenderer>();
            prtr.line = lineRenderer;
            item.tail = prtr;
            lineRenderer.material.SetColor("_Color", (Color)config.baseColor);
            lineRenderer.material.SetColor("_BaseColor", (Color)config.baseColor);
            lineRenderer.startColor = (Color)config.baseColor;
            lineRenderer.endColor = (Color)config.baseColor;
        }
        item.gameObject.SetActive(true);
        item.Launch(config, baseDir, rot);
    }
}
