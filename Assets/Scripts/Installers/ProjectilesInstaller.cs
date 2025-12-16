using UnityEngine;
using Zenject;

public class ProjectilesInstaller : MonoInstaller
{
    public Projectile defaultPrefab;
    public override void InstallBindings()
    {
        TextAsset[] configs = Resources.LoadAll<TextAsset>("Configs/Projectiles");

        foreach (var config in configs)
        {
            ProjectileConfig projConf = JsonUtility.FromJson<ProjectileConfig>(config.text);
            GameObject prefab = Resources.Load<GameObject>(projConf.pathToModel);
            if (prefab == null)
            {
                Debug.LogError($"Не найден префаб по пути {projConf.pathToModel}");
                continue;
            }
            GameObject _modelInstance = GameObject.Instantiate(prefab);
            Projectile proj =_modelInstance.AddComponent<Projectile>();
            Rigidbody rb = _modelInstance.AddComponent<Rigidbody>();
            MeshCollider col = _modelInstance.AddComponent<MeshCollider>();
            col.convex = true;
            rb.useGravity = false;
            proj.rb = rb;
            Container.BindMemoryPool<Projectile, ProjectilePool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(_modelInstance)
                .UnderTransformGroup("Projectiles")
                .WhenInjectedInto<Weapon>();
        }
    }
}
