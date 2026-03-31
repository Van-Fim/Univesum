using UnityEngine;
using Zenject;

public class ProjectilesInstaller : MonoInstaller
{
    public Transform spaceContainer;
    public override void InstallBindings()
    {
        ProjectileConfig[] configs = JsonConfigLoader.LoadAllFromFolder<ProjectileConfig>("Projectiles");

        foreach (var config in configs)
        {
            GameObject projFX = Resources.Load<GameObject>("Prefabs/ProjectileFx");
            GameObject prefab = Resources.Load<GameObject>(config.pathToModel);
            if (prefab == null)
            {
                Debug.LogError($"Не найден префаб по пути {config.pathToModel}");
                continue;
            }
            GameObject _modelInstance = GameObject.Instantiate(projFX);
            Projectile proj = _modelInstance.AddComponent<Projectile>();
            Rigidbody rb = _modelInstance.AddComponent<Rigidbody>();
            MeshCollider col = _modelInstance.AddComponent<MeshCollider>();
            col.convex = true;

            if (proj.beam == null)
            {
                proj.beam = proj.transform.Find("Beam").GetComponent<ParticleSystem>();

                var beamMain = proj.beam.main;
                beamMain.startColor = new ParticleSystem.MinMaxGradient(config.baseColor);
            }
            if (proj.body == null)
            {
                proj.body = proj.transform.Find("Body").GetComponent<ParticleSystem>();

                var bodyMain = proj.body.main;
                bodyMain.startColor = new ParticleSystem.MinMaxGradient(config.baseColor);
            }
            if (proj.explode == null)
            {
                proj.explode = proj.transform.Find("Explode").GetComponent<ParticleSystem>();

                var expMain = proj.explode.main;
                expMain.startColor = new ParticleSystem.MinMaxGradient(config.baseColor);
            }
            MeshFilter mf = prefab.GetComponent<MeshFilter>();
            if (mf != null)
            {
                col.sharedMesh = mf.sharedMesh;
                Debug.Log(proj.body);
                ParticleSystemRenderer psr = proj.body.GetComponent<ParticleSystemRenderer>();
                psr.mesh = mf.sharedMesh;
            }
            else
            {
                Debug.LogError("У префаба нет MeshFilter, не могу назначить MeshCollider");
            }

            rb.useGravity = false;
            proj.rb = rb;
            Container.BindMemoryPool<Projectile, ProjectilePool>()
                .WithInitialSize(3)
                .FromComponentInNewPrefab(_modelInstance)
                .UnderTransformGroup("SpaceContainer")
                .WhenInjectedInto<Weapon>();
            _modelInstance.gameObject.SetActive(false);
        }
    }
}
