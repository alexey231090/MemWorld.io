using UnityEngine;
using UnityEngine.Events;

public class SteeringWheelUse : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private float steeringWheelDetectionDistance = 2f;
    [SerializeField] private LayerMask steeringWheelLayer;
     public bool isPlayerControllerEnabled { get; private set; } = true;
    


    RotationCamera rotationCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rotationCamera = FindFirstObjectByType<RotationCamera>();
    
        if (rotationCamera == null)
        {
            Debug.LogError("[SteeringWheelUse] RotationCamera not found!");
        }

        // Сначала проверяем, не смотрим ли мы на рулевое колесо
			if (CheckForSteeringWheel())
			{
				// Если нашли руль, обработка завершается
				return;
			}
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.E))
		{
			// Если уже в режиме управления кораблём, выходим без проверки взгляда
			if (!isPlayerControllerEnabled)
			{
				playerController.enabled = true;
				isPlayerControllerEnabled = true;
				if (debugLogs)
					Debug.Log("[SteeringWheelUse] PlayerController включен по нажатию E (выход из управления кораблём)");

				rotationCamera.enabled = false;
				return;
			}

			// Иначе пытаемся войти в управление: требуется смотреть на руль
			if (CheckForSteeringWheel())
			{
				return;
			}
		}
    }


    private bool CheckForSteeringWheel()
	{
		if (playerCamera == null || playerController == null)
			return false;

		// Пускаем рейкаст из центра камеры
		Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

		// Проверяем, попадает ли рейкаст на объект со слоем SteeringWheel
		if (Physics.Raycast(ray, out RaycastHit hit, steeringWheelDetectionDistance, steeringWheelLayer, QueryTriggerInteraction.Ignore))
		{
			// Если PlayerController включен, отключаем его
			if (isPlayerControllerEnabled)
			{
				playerController.enabled = false;
				isPlayerControllerEnabled = false;
				if (debugLogs)
					Debug.Log("[PlayerGrab] PlayerController отключен при нажатии ЛКМ на рулевое колесо");

				rotationCamera.enabled = true;
                
			}
			else
			{
				// Если PlayerController отключен, включаем его
				playerController.enabled = true;
				isPlayerControllerEnabled = true;
				if (debugLogs)
					Debug.Log("[PlayerGrab] PlayerController включен при нажатии ЛКМ на рулевое колесо");

				rotationCamera.enabled = false;
                
			}
			return true; // Нашли руль и обработали
		}

		return false; // Не нашли руль
	}
}
