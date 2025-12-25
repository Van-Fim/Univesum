using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileConfig config;
    public Rigidbody rb;
    public Weapon weapon;

    public ParticleSystem beam;
    public ParticleSystem body;
    bool is_initialized;

    private float destroyTime;
    public void Init()
    {
        if (is_initialized) return;
        is_initialized = true;
        weapon._signalBus.Subscribe<SignalChunkFloatingOriginFix>(OnChunkFloatingOriginFix);
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

        // Schedule self-destruction after lifetime
        destroyTime = Time.time + config.lifetime;
        Invoke("SelfDestruct", config.lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        // урон
        // col.gameObject.GetComponent<Health>()?.TakeDamage(damage);

        // Cancel pending self-destruction
        if (weapon == null)
        {
            return;
        }
        CancelInvoke("SelfDestruct");
        Explode();
    }
    void Explode()
    {
        // какой нибудь эффект столкновения
        SelfDestruct();
    }
    void SelfDestruct()
    {
        weapon._pool.Despawn(this);
    }
}
