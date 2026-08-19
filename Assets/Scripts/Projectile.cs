using UnityEngine;

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

    private float destroyTime;
    public void Init()
    {
        if (is_initialized) return;
        is_initialized = true;
        objCollider = GetComponent<Collider>();
        objCollider.isTrigger = true;
        weapon._signalBus.Subscribe<SignalChunkFloatingOriginFix>(OnChunkFloatingOriginFix);
        Debug.Log("Projectile initialized");

    }
    public void OnChunkFloatingOriginFix(SignalChunkFloatingOriginFix signal)
    {
        transform.localPosition -= signal.offset;
    }
    public void Launch(ProjectileConfig config, Vector3 direction, Quaternion rotation)
    {
        this.config = config;

        Vector3 v1 = weapon._parent.rigidbody.linearVelocity;
        rb.linearVelocity = direction * (this.config.speed) + v1;

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
}
