using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileConfig config;
    public Rigidbody rb;
    public Weapon weapon;

    public void Launch(ProjectileConfig config, Vector3 direction, Quaternion rotation)
    {
        this.config = config;
        Vector3 v1 = weapon._parent.rigidbody.linearVelocity;
        rb.linearVelocity = direction * (this.config.speed);
    }

    void OnCollisionEnter(Collision col)
    {
        // урон
        // col.gameObject.GetComponent<Health>()?.TakeDamage(damage);
        gameObject.SetActive(false); // возвращаем в пул
    }
}
