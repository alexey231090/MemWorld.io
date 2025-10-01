using UnityEngine;
public class EnemyShooter : MonoBehaviour
{
    [Header("Shoot Settings")]
    [SerializeField] GameObject bullet;
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] float maxLifeTime = 5f;
    [SerializeField] Transform muzzle; // точка спавна пули (дуло/пушка)
    [SerializeField] LayerMask projectileHitLayers = ~0; // слои, по которым пуля будет попадать
    [SerializeField] float damageEnemyShooter = 3f;

    [Header("Target Detection")]
    [SerializeField] LayerMask targetLayers = ~0; // слои целей для поиска
    [SerializeField] float detectionRadius = 15f; // радиус обнаружения целей
    [SerializeField] float shootInterval = 1f; // интервал между выстрелами
    [SerializeField] float maxShootDistance = 50f; // максимальная дальность стрельбы

    [Header("Angle Restrictions")]
    [SerializeField] float maxHorizontalAngle = 45f; // максимальный угол по горизонтали (в градусах)
    [SerializeField] bool useAngleRestrictions = true; // включить/выключить ограничения углов

    [Header("Gun Direction")]
    [SerializeField] Transform gunForwardTransform; // Transform для направления пушки (если null - используется transform.forward)
    [SerializeField] bool showGizmos = true; // показывать ли Gizmos в Scene View
    [SerializeField] Color gizmoColor = Color.red; // цвет Gizmos
    [SerializeField] Color detectionGizmoColor = Color.yellow; // цвет сферы обнаружения

    private Transform currentTarget;
    private float lastShootTime;

    void Start()
    {
        lastShootTime = -shootInterval; // разрешаем стрельбу сразу
    }

    void Update()
    {
        // Ищем ближайшую цель
        FindNearestTarget();

        // Стреляем по цели, если она найдена и в пределах угла
        if (currentTarget != null && CanShoot())
        {
            TryShoot();
        }
    }

    private void FindNearestTarget()
    {
        // Ищем все коллайдеры в радиусе обнаружения
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayers);

        Transform nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (col.transform == transform) continue; // не стреляем в себя

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < nearestDistance && distance <= maxShootDistance)
            {
                nearestTarget = col.transform;
                nearestDistance = distance;
            }
        }

        currentTarget = nearestTarget;
    }

    private bool CanShoot()
    {
        // Проверяем интервал стрельбы
        if (Time.time - lastShootTime < shootInterval)
        {
            return false;
        }

        // Проверяем ограничения углов
        if (useAngleRestrictions && currentTarget != null)
        {
            Vector3 targetDirection = (currentTarget.position - transform.position).normalized;
            if (!IsTargetWithinAngleRange(targetDirection))
            {
                return false;
            }
        }

        return true;
    }

    private void TryShoot()
    {
        if (bullet == null || currentTarget == null)
        {
            return;
        }

        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;
        Vector3 targetPos = currentTarget.position;
        Vector3 direction = (targetPos - spawnPos).normalized;

        GameObject b = Instantiate(bullet, spawnPos, Quaternion.LookRotation(direction, Vector3.up));
        // Передаём параметры пули в Projectile
        var proj = b.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetHitLayers(projectileHitLayers);
            proj.SetDamage(damageEnemyShooter);
            proj.SetFlightParameters(bulletSpeed, maxLifeTime, targetPos);
        }

        lastShootTime = Time.time;
    }


    // Проверяет, находится ли цель в допустимом диапазоне углов
    private bool IsTargetWithinAngleRange(Vector3 targetDirection)
    {
        // Получаем направление "вперёд" пушки
        Vector3 gunForward = GetGunForward();

        // Получаем локальные оси пушки
        Vector3 gunRight = transform.right;
        Vector3 gunUp = transform.up;

        // Проектируем направления на локальные оси пушки
        Vector3 horizontalGunForward = Vector3.ProjectOnPlane(gunForward, gunUp).normalized;
        Vector3 horizontalTargetDir = Vector3.ProjectOnPlane(targetDirection, gunUp).normalized;
        float horizontalAngle = Vector3.Angle(horizontalGunForward, horizontalTargetDir);

        // Проверяем, что углы не превышают ограничения
        bool horizontalOK = horizontalAngle <= maxHorizontalAngle;

        return horizontalOK;
    }

    // Получает направление "вперёд" пушки
    private Vector3 GetGunForward()
    {
        if (gunForwardTransform != null)
        {
            return gunForwardTransform.forward;
        }
        return transform.forward;
    }

    // Рисует Gizmos для визуализации направления пушки и ограничений углов
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 gunForward = GetGunForward();
        Vector3 gunPosition = transform.position;

        // Рисуем сферу обнаружения
        Gizmos.color = new Color(detectionGizmoColor.r, detectionGizmoColor.g, detectionGizmoColor.b, 0.2f);
        Gizmos.DrawSphere(gunPosition, detectionRadius);

        // Рисуем границу сферы обнаружения
        Gizmos.color = detectionGizmoColor;
        Gizmos.DrawWireSphere(gunPosition, detectionRadius);

        // Рисуем основное направление пушки
        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(gunPosition, gunForward * 3f);

        // Рисуем конус ограничений углов
        if (useAngleRestrictions)
        {
            DrawAngleCone(gunPosition, gunForward);
        }

        // Рисуем линию к текущей цели
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(gunPosition, currentTarget.position);
        }
    }

    // Рисует конус ограничений углов
    private void DrawAngleCone(Vector3 position, Vector3 forward)
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);

        // Создаем точки для конуса
        int segments = 16;
        float radius = 2f;

        // Горизонтальный конус
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i * 360f / segments) * Mathf.Deg2Rad;
            float angle2 = ((i + 1) * 360f / segments) * Mathf.Deg2Rad;

            Vector3 dir1 = Quaternion.AngleAxis(maxHorizontalAngle, Vector3.up) * forward;
            Vector3 dir2 = Quaternion.AngleAxis(-maxHorizontalAngle, Vector3.up) * forward;

            Vector3 point1 = position + dir1 * radius;
            Vector3 point2 = position + dir2 * radius;

            Gizmos.DrawLine(position, point1);
            Gizmos.DrawLine(position, point2);
        }
    }
}
