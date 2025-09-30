using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 90f;
    public float turnSpeed = 45f;
    
    [Header("Inertia Settings")]
    public float inertia = 0.99f; // Инерция после отпускания газа (0-1)

    private Vector3 velocity = Vector3.zero;

    bool isMove = false;

    SteeringWheelUse steeringWheel;

    void Awake()
    {
        steeringWheel = FindFirstObjectByType<SteeringWheelUse>();
        if(steeringWheel == null)
        {
            Debug.LogError("[MovePlatform] SteeringWheelUse not found!");
        }
    }



    void Update()
    {
        HandleInput();
        ApplyMovement();
    }

    void HandleInput()
    {
        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);

        if(!steeringWheel.isPlayerControllerEnabled)
        {
            isMove = true;
        }
        else
        {
            isMove = false;
        }

        // Движение вперёд/назад
        if(isMove)
        {
            if (forward)
            {
                velocity += transform.forward * moveSpeed * Time.deltaTime;
            }
            else if (backward)
            {
                velocity -= transform.forward * moveSpeed * Time.deltaTime;
            }

            // Поворот
            if (left && !forward && !backward) // Поворот на месте
            {
                transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
            }
            else if (right && !forward && !backward) // Поворот на месте
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
            else if (left && forward) // Поворот влево при движении вперёд
            {
                transform.Rotate(0, -turnSpeed * Time.deltaTime, 0);
            }
            else if (right && forward) // Поворот вправо при движении вперёд
            {
                transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
            }
            else if (left && backward) // Поворот влево при движении назад
            {
                transform.Rotate(0, turnSpeed * Time.deltaTime, 0); // Обратный поворот
            }
            else if (right && backward) // Поворот вправо при движении назад
            {
                transform.Rotate(0, -turnSpeed * Time.deltaTime, 0); // Обратный поворот
            }
        }
    }

    void ApplyMovement()
    {
        // Применяем движение
        transform.position += velocity * Time.deltaTime;
        
        // Применяем инерцию (замедление после отпускания газа)
        velocity *= inertia;
        
        // Останавливаем очень медленное движение
        if (velocity.magnitude < 0.01f)
        {
            velocity = Vector3.zero;
        }
    }

}


    // Переменная для хранения состояния движения с руля
    
