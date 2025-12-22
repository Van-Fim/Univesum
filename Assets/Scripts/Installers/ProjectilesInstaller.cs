using UnityEngine;
using Zenject;

public class ProjectilesInstaller : MonoInstaller
{
    public Transform spaceContainer;
    public override void InstallBindings()
    {
        TextAsset[] configs = Resources.LoadAll<TextAsset>("Configs/Projectiles");

        foreach (var config in configs)
        {
            ProjectileConfig projConf = JsonUtility.FromJson<ProjectileConfig>(config.text);
            GameObject projFX = Resources.Load<GameObject>("Prefabs/ProjectileFx");
            GameObject prefab = Resources.Load<GameObject>(projConf.pathToModel);
            if (prefab == null)
            {
                Debug.LogError($"Не найден префаб по пути {projConf.pathToModel}");
                continue;
            }
            GameObject _modelInstance = GameObject.Instantiate(projFX);
            Projectile proj =_modelInstance.AddComponent<Projectile>();
            Rigidbody rb = _modelInstance.AddComponent<Rigidbody>();
            MeshCollider col = _modelInstance.AddComponent<MeshCollider>();
            col.convex = true;
            rb.useGravity = false;
            proj.rb = rb;
            Container.BindMemoryPool<Projectile, ProjectilePool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_modelInstance)
                .UnderTransformGroup("SpaceContainer")
                .WhenInjectedInto<Weapon>();
        }
    }
}
