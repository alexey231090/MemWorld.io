using UnityEngine;

public class RotationCamera : MonoBehaviour
{
    [SerializeField] private bool cameraEnabled = true;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool lockCursor = true;

    [SerializeField] private Transform playerBody; // Тело персонажа для вращения по горизонтали

    private float xRotation = 0f;
    private bool cursorWasLocked = false;

    private void Awake()
    {
        // Если тело персонажа не назначено, пытаемся найти его в родительских объектах
        if (playerBody == null)
        {
            playerBody = transform.parent;

            if (playerBody == null)
            {
                Debug.LogWarning("[RotationCamera] Player body not assigned and not found as parent!", this);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Изначально скрываем курсор, если это настроено
        ApplyCursorLock(lockCursor);
    }

    // Update is called once per frame
    void Update()
    {
        HandleCursorToggle();
        if (!cameraEnabled) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Вертикальный взгляд (питч)
        xRotation += (invertY ? mouseY : -mouseY);
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Горизонтальный поворот тела (йоу)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX, Space.Self);
        }
    }

    private void HandleCursorToggle()
    {
        // Toggle по Esc: разблокировка курсора
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool newLocked = !(Cursor.lockState == CursorLockMode.Locked);
            ApplyCursorLock(newLocked);
        }

        // Если параметр lockCursor включён и курсор внезапно разблокирован, вернуть блокировку
        if (lockCursor && !cursorWasLocked && Cursor.lockState != CursorLockMode.Locked)
        {
            ApplyCursorLock(true);
        }

        cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
    }

    private void ApplyCursorLock(bool shouldLock)
    {
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
        cursorWasLocked = shouldLock;
    }
}
