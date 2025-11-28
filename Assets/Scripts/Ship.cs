using UnityEngine;

public class Ship : SpaceObject
{
    public Engine engine;

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
                engine = JsonConfigLoader.LoadFromResources<Engine>("Configs/Engines/" + hp.item);
                if (hardpoints != null && hardpoints.childCount > 0)
                {
                    for (int j = 0; j < hardpoints.childCount; j++)
                    {
                        Transform tr = hardpoints.GetChild(j);
                        if (tr.name.StartsWith("HPFXEngine"))
                        {
                            // TrailRenderer trail = GameObject.Instantiate(Resources.Load<TrailRenderer>("Prefabs/EngineTrail"));
                            // trail.transform.SetParent(tr);
                            // trail.transform.localPosition = Vector3.zero;
                            // trail.transform.localRotation = Quaternion.identity;

                            GameObject JetEngineGameObject = _container.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/JetEngine"));
                            JetEngineController JetEngine = JetEngineGameObject.GetComponent<JetEngineController>();
                            JetEngine.transform.SetParent(tr);
                            JetEngine.transform.localPosition = Vector3.zero;
                            JetEngine.transform.localRotation = Quaternion.identity;
                            JetEngine.ApplyGradient(engine.color01, engine.color02);
                        }
                    }
                }
            }
        }
    }
}
