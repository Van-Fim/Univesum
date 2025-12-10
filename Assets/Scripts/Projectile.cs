using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileConfig config;
    public Rigidbody rb;

    public void Launch(Vector3 direction)
    {
        rb.linearVelocity = direction * config.speed;
    }

    void OnCollisionEnter(Collision col)
    {
        // урон
        // col.gameObject.GetComponent<Health>()?.TakeDamage(damage);
        gameObject.SetActive(false); // возвращаем в пул
    }
}
