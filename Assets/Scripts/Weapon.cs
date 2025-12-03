using UnityEngine;
using Zenject;
public class Weapon : MonoBehaviour
{
    public WeaponConfig _config;
    public Ship _parent;
    protected GameObject main = null;
    [Inject]
    public void Construct(Ship parent, WeaponConfig config)
    {
        _parent = parent;
        _config = config;
    }
    public virtual void InstallConfig()
    {
        if (this.main != null)
        {
            return;
        }
        GameObject gm = Resources.Load<GameObject>(_config.pathToModel);

        main = GameObject.Instantiate<GameObject>(gm, transform);
        main.transform.localPosition = Vector3.zero;
        main.transform.localEulerAngles = Vector3.zero;
        main.name = "MAIN";
    }
}