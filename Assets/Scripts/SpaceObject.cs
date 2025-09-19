using Unity.Collections;
using UnityEngine;
using Zenject;
public abstract class SpaceObject : MonoBehaviour
{
    public SpaceObjectConfig spaceObjectConfig;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected MeshCollider meshCollider;
    [Inject] protected CameraManager cameraManager;
    public Rigidbody rigidbody;
    public Transform hardpoints;
    protected Mesh mesh;

    protected GameObject main = null;

    public virtual void Init()
    {
        Hide();
    }
    public virtual void InstallHardpoints(string path)
    {
        if (path == null)
        {
            return;
        }
        Transform o1 = Resources.Load<Transform>(path);
        o1 = GameObject.Instantiate(o1.transform, transform);
        o1.transform.localPosition = Vector3.zero;
        o1.transform.rotation = Quaternion.identity;
        o1.name = "HARDPOINTS";
        hardpoints = o1;
    }
    public virtual void InstallCamera()
    {
        Transform camHardpoint = hardpoints.Find("CAMERA");
        Camera cam = cameraManager.GetMainCamera();
        cam.transform.SetParent(camHardpoint);
        cam.transform.localPosition = Vector3.zero;
        cam.transform.rotation = Quaternion.identity;
    }

    public virtual void InstallConfig(SpaceObjectConfig config)
    {
        if (this.main != null)
        {
            return;
        }
        spaceObjectConfig = config;
        if (rigidbody != null)
            rigidbody.mass = config.mass;

        GameObject gm = Resources.Load<GameObject>(config.pathToModel);
        if (config.chinldName.Length > 0)
        {
            var tr = gm.transform.Find(config.chinldName);
            if (tr != null)
            {
                gm = tr.gameObject;
            }
        }
        main = GameObject.Instantiate<GameObject>(gm, transform);
        main.transform.localPosition = Vector3.zero;
        main.transform.localEulerAngles = Vector3.zero;
        main.name = "MAIN";
        main.AddComponent<MeshCollider>();
        gameObject.AddComponent<Rigidbody>();
        meshRenderer = main.GetComponent<MeshRenderer>();
        main.transform.localScale = new Vector3(config.scale, config.scale, config.scale);
        if (meshRenderer == null)
        {
            meshRenderer = main.AddComponent<MeshRenderer>();
        }
        meshFilter = main.GetComponent<MeshFilter>();
        meshCollider = main.GetComponent<MeshCollider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = config.mass;
        rigidbody.linearDamping = config.linearDrag;
        rigidbody.angularDamping = config.angularDrag;
        meshCollider.convex = true;
        rigidbody.useGravity = false;

        if (config.pathToMaterial != null && config.pathToMaterial.Length > 0)
        {
            Material mat = Resources.Load<Material>(config.pathToMaterial);
            meshRenderer.material = mat;
        }

        InstallHardpoints(config.pathToHardpoints);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
