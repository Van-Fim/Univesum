using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileConfig config;
    public Rigidbody rb;
    public Weapon weapon;

    public ParticleSystem beam;
    public ParticleSystem body;

    [Header("Optional tail")]
    public ProjectileTailLineRenderer tail;

    private float destroyTime;

    public void Launch(ProjectileConfig config, Vector3 direction, Quaternion rotation)
    {
        this.config = config;

        // Reset tail for pooling
        if (tail != null)
        {
            tail.ApplyConfig(config);
            tail.ResetTail();
        }

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
        // Clean tail for pooling (avoid showing previous trail on next spawn)
        if (tail != null)
            tail.ResetTail();

        weapon._pool.Despawn(this);
    }
}
