using UnityEngine;

public class Projectile : MonoBehaviour
{

    private MemSubject memSubject;

    [Header("Projectile Settings")]
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float damageProjectile = 1f;
    
    [Header("Hit Layers")]
    [SerializeField] private LayerMask hitLayers = ~0; // слои, по которым эта пуля будет попадать (задаётся стрелком)

    // Позволяет стрелку задать слои на лету
    public void SetHitLayers(LayerMask layers)
    {
        hitLayers = layers;
    }

    // Позволяет стрелку задать урон на лету
    public void SetDamage(float damage)
    {
        damageProjectile = damage;
    }

    
    

    // Обработка столкновений с триггерами
    void OnTriggerEnter(Collider other)
    {
        if (destroyOnHit)
        {
            // Проверяем, можно ли попадать по этому слою
            if (((1 << other.gameObject.layer) & hitLayers) != 0)
            {
                // Наносим урон, если есть MemSubject
                memSubject = other.GetComponentInParent<MemSubject>();
                if (memSubject != null)
                {
                    memSubject.ApplyDamage(damageProjectile);
                }
                
                Destroy(gameObject);
            }
        }
    }

    // Обработка столкновений с обычными коллайдерами
    void OnCollisionEnter(Collision collision)
    {
        if (destroyOnHit)
        {
            // Проверяем, можно ли попадать по этому слою
            if (((1 << collision.gameObject.layer) & hitLayers) != 0)
            {
                // Наносим урон, если есть MemSubject
                memSubject = collision.gameObject.GetComponentInParent<MemSubject>();
                if (memSubject != null)
                {
                    memSubject.ApplyDamage(damageProjectile);
                }
                Destroy(gameObject);
            }
        }
    }
}
