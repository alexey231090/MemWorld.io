using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{

    private MemSubject memSubject;

    [Header("Projectile Settings")]
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float damageProjectile = 1f;// урон, который наносит пуля

    [Header("Hit Layers")]
    [SerializeField] private LayerMask hitLayers = ~0; // слои, по которым эта пуля будет попадать (задаётся стрелком)

    // Настройки полёта (задаются стрелком)
    private float bulletSpeed = 20f;
    private float maxLifeTime = 5f;
    private Vector3 targetPoint;
    private bool isInitialized = false;

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

    // Позволяет стрелку задать параметры полёта
    public void SetFlightParameters(float speed, float lifeTime, Vector3 target)
    {
        bulletSpeed = speed;
        maxLifeTime = lifeTime;
        targetPoint = target;
        isInitialized = true;

        // Запускаем полёт
        StartCoroutine(FlightRoutine());
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

    // Корутина полёта пули
    private IEnumerator FlightRoutine()
    {
        if (!isInitialized) yield break;

        float life = 0f;
        Vector3 startPos = transform.position;
        Vector3 direction = (targetPoint - startPos).normalized;

        while (life < maxLifeTime && gameObject != null)
        {
            if (gameObject != null)
            {
                transform.position += direction * bulletSpeed * Time.deltaTime;
            }
            life += Time.deltaTime;
            yield return null;
        }

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
