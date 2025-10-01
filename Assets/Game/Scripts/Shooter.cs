using UnityEngine;
using EvolveGames;

public class Shooter : MonoBehaviour
{

    [Header("Virable")]
    [SerializeField] GameObject bullet;
    [SerializeField] Camera playerCamera; // камера игрока для рейкаста

    private PlayerController playerController;
    private SteeringWheelUse steeringWheelUse;

    [Header("Shoot Settings")]
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] float maxDistance = 100f; // максимальная дальность рейкаста
    [SerializeField] float maxLifeTime = 5f;
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] Transform muzzle; // точка спавна пули (дуло/пушка)
    [SerializeField] LayerMask projectileHitLayers = ~0; // слои, по которым пуля будет попадать
    [SerializeField] float damageShooter = 3f;

    [Header("Angle Restrictions")]
    [SerializeField] float maxHorizontalAngle = 45f; // максимальный угол по горизонтали (в градусах)
    [SerializeField] bool useAngleRestrictions = true; // включить/выключить ограничения углов

    [Header("Gun Direction")]
    [SerializeField] Transform gunForwardTransform; // Transform для направления пушки (если null - используется transform.forward)
    [SerializeField] bool showGizmos = true; // показывать ли Gizmos в Scene View
    [SerializeField] Color gizmoColor = Color.red; // цвет Gizmos




    void Awake()
    {
        Init();
    }
    void Update()
    {
        if (Input.GetKeyDown(shootKey))
        {
            TryShoot();
        }
    }


    void Init()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        playerCamera = playerController.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("[Shooter] Player camera not found!");
        }

        steeringWheelUse = FindFirstObjectByType<SteeringWheelUse>();
        if (steeringWheelUse == null)
        {
            Debug.LogError("[Shooter] SteeringWheelUse not found!");
        }
    }
    void TryShoot()
    {
        if (bullet == null || playerCamera == null)
        {
            return;
        }

        // Стреляем только когда игрок контролирует корабль (обычный режим)
        if (steeringWheelUse != null && steeringWheelUse.isPlayerControllerEnabled)
        {
            return; // Стреляем в режиме управления кораблём
        }

        // Рейкаст из камеры игрока
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;
        Vector3 targetPoint = origin + direction * maxDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            targetPoint = hit.point;
        }

        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;
        Vector3 dir = (targetPoint - spawnPos).normalized;

        // Проверяем ограничения углов
        if (useAngleRestrictions && !IsTargetWithinAngleRange(dir))
        {
            Debug.Log("[Shooter] Цель вне допустимого угла обстрела");
            return;
        }

        GameObject b = Instantiate(bullet, spawnPos, Quaternion.LookRotation(dir, Vector3.up));
        // Передаём параметры пули в Projectile
        var proj = b.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetHitLayers(projectileHitLayers);
            proj.SetDamage(damageShooter);
            proj.SetFlightParameters(bulletSpeed, maxLifeTime, targetPoint);
        }
    }



    // Проверяет, находится ли цель в допустимом диапазоне углов
    private bool IsTargetWithinAngleRange(Vector3 targetDirection)
    {
        // Получаем направление "вперёд" пушки
        Vector3 gunForward = GetGunForward();

        // Вычисляем угол между направлением пушки и направлением к цели
        float angle = Vector3.Angle(gunForward, targetDirection);

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

        Gizmos.color = gizmoColor;

        // Рисуем основное направление пушки
        Gizmos.DrawRay(gunPosition, gunForward * 3f);

        // Рисуем конус ограничений углов
        if (useAngleRestrictions)
        {
            DrawAngleCone(gunPosition, gunForward);
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
