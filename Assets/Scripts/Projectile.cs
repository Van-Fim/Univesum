using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public ProjectileConfig config;
    public Rigidbody rb;
    public Weapon weapon;

    public ParticleSystem beam;
    public ParticleSystem explode;
    public ParticleSystem body;
    bool is_initialized;
    bool is_collided;
    Collider objCollider;

    public SpaceObject _parent;

    public static UnityAction<SpaceObject> OnCheckParentAction;
    public static List<Collider> ignoreList = new List<Collider>();
    public Collider spaceObjectCollider;

    private float destroyTime;
    public void Init()
    {
        if (is_initialized) return;
        is_initialized = true;
        objCollider = GetComponent<Collider>();
        objCollider.isTrigger = true;
        weapon._signalBus.Subscribe<SignalChunkFloatingOriginFix>(OnChunkFloatingOriginFix);
        float sc = body.main.startSize.constant;
        objCollider.transform.localScale = new Vector3(sc, sc, sc);
        OnCheckParentAction += OnCheckParent;
        Collider col = this.GetComponent<Collider>();
        for (int i = 0; i < ignoreList.Count; i++)
        {
            Physics.IgnoreCollision(col, ignoreList[i]);
        }
        if (!ignoreList.Contains(col))
        {
            ignoreList.Add(col);
        }
    }
    public void OnCheckParent(SpaceObject parent)
    {

    }
    public void OnChunkFloatingOriginFix(SignalChunkFloatingOriginFix signal)
    {
        transform.localPosition -= signal.offset;
    }
    public void Launch(ProjectileConfig config, Vector3 direction, Quaternion rotation)
    {
        this.config = config;

        // Рассчитываем, насколько скорость корабля совпадает с направлением выстрела
        Vector3 shipVelocity = weapon._parent.rigidbody.linearVelocity;
        float forwardImpulse = Vector3.Dot(shipVelocity, direction);

        rb.linearVelocity = direction * (this.config.speed + forwardImpulse);

        transform.rotation = Quaternion.LookRotation(rb.linearVelocity);

        destroyTime = Time.time + config.lifetime;
        Invoke("SelfDestruct", config.lifetime);
    }

    void OnTriggerEnter(Collider col)
    {
        if (weapon == null)
        {
            return;
        }
        SpaceObject sp = col.gameObject.GetComponent<SpaceObject>();
        CoreCollider coreCollider = col.gameObject.GetComponent<CoreCollider>();
        if (coreCollider != null)
        {
            sp = coreCollider._parent;
        }
        if (sp != null && !is_collided)
        {
            sp.InvokeTakeDamage(weapon._parent, (int)config.damage);
            is_collided = true;
        }
        CancelInvoke("SelfDestruct");
        Explode();
    }
    void Explode()
    {
        beam.Stop();
        body.Stop();
        if (!explode.isPlaying)
        {
            var expMain = explode.main;
            expMain.duration = config.explodeTime;
            explode.Play();
        }
        rb.isKinematic = true;

        Invoke("SelfDestruct", config.explodeTime);
    }
    void SelfDestruct()
    {
        rb.isKinematic = false;
        is_collided = false;

        weapon._pool.Despawn(this);
    }
    public static void InvokeOnCheckParent(SpaceObject parent)
    {
        OnCheckParentAction?.Invoke(parent);
    }
}
