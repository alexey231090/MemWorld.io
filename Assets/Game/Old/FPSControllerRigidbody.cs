using UnityEngine;

/// <summary>
/// Управление игроком от первого лица с использованием Rigidbody.
/// Предоставляет базовое FPS-управление без использования CharacterController.
/// </summary>
public class FPSControllerRigidbody : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    // Private variables
    private Vector3 moveInput;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private bool isGrounded;

    private void Reset()
    {
        // Поиск компонентов при добавлении скрипта
        playerRigidbody = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        // Создание точки проверки земли, если она не существует
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheck = checkObj.transform;
        }

        // Настройка Rigidbody
        if (playerRigidbody != null)
        {
            playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Установка слоя земли по умолчанию
        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Start()
    {
        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Инициализация поворота
        horizontalRotation = transform.eulerAngles.y;
    }

    private void Update()
    {
        // Получаем ввод от игрока
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Формируем направление движения относительно камеры
        Vector3 cameraForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        Vector3 cameraRight = playerCamera != null ? playerCamera.transform.right : transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        moveInput = (cameraForward * vertical + cameraRight * horizontal).normalized;

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Управление камерой
        HandleCameraRotation();
    }

    private void FixedUpdate()
    {
        // Проверка на землю
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Движение с помощью Rigidbody
        if (moveInput != Vector3.zero)
        {
            // Применяем силу движения
            Vector3 targetVelocity = moveInput * moveSpeed;
            Vector3 currentVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
            Vector3 velocityChange = targetVelocity - currentVelocity;

            // Применяем ускорение для более плавного движения
            playerRigidbody.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
        }
        else
        {
            // Останавливаем горизонтальное движение, если нет ввода
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
        }
    }

    private void HandleCameraRotation()
    {
        // Получаем ввод мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Горизонтальное вращение (вращение всего игрока)
        horizontalRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        // Вертикальное вращение (вращение только камеры)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    // Метод для включения/выключения контроллера
    public void SetControllerEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = !enabled;
        }
    }
}
