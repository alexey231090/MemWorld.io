using UnityEngine;
using DG.Tweening;
using EvolveGames;

/// <summary>
/// Simple physics grabber: LMB to grab object in the center of the camera, LMB again to release.
/// Attach this to your PlayerController and assign the player Camera.
/// </summary>
public class PlayerGrab : MonoBehaviour
{
	 
	[Header("Initialization")]
	[SerializeField] private Camera playerCamera;
	[SerializeField] private Transform hand; // точка крепления (дочерний объект Player: Hand)
	[SerializeField] private PlayerController playerController; // скрипт управления персонажем
	private MemSubject memSubject;

	[Header("Grab Settings")]
	[SerializeField] private float maxGrabDistance = 8f;
	[SerializeField] private float holdDistance = 3f; // используется только если нет hand
	[SerializeField] private float pullDuration = 0.25f;
	[SerializeField] private Ease pullEase = Ease.OutCubic;
	
	[Header("Throw Settings")]
	[SerializeField] private float throwForce = 15f; // сила броска
	[SerializeField] private float throwUpwardForce = 5f; // дополнительная сила вверх
	

	[Header("Layer Settings")]
	[SerializeField] private LayerMask steeringWheelLayer; // слой для рулевого колеса
	[SerializeField] private LayerMask grabbableMask = ~0; // default: everything
	[SerializeField] private LayerMask platsLayer; // слой для платформ
	
	[Header("Plats Detection")]
	[SerializeField] private float platsDetectionDistance = 5f; // дальность обнаружения платформ

	[Header("While Held Physics")]
	[SerializeField] private bool alignRotationOnAttach = true;
	[SerializeField] private float heldDrag = 10f;
	[SerializeField] private float heldAngularDrag = 10f;

	private enum GrabState { None, Pulling, Held }
	private GrabState grabState = GrabState.None;

	private Rigidbody heldBody;
	private bool originalUseGravity;
	private float originalDrag;
	private float originalAngularDrag;
	private bool originalIsKinematic;
	private RigidbodyConstraints originalConstraints;
	private Collider[] originalColliders;
	private bool[] originalColliderStates;
	private Tween currentTween;

	public MemSubject.MemSubjectType memSubjectInHand = MemSubject.MemSubjectType.None;

	



	void Awake()
	{
			playerCamera = GetComponentInChildren<Camera>();

			playerController = GetComponent<PlayerController>();
			
			Init();
	}

	

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// Сначала проверяем платформы при нажатии ЛКМ
			CheckForPlats();
			
			// Если не руль, то обрабатываем обычное схватывание
			if (grabState == GrabState.None)
			{
				TryGrab();
			}
			
		}
		
		// Выбрасывание предмета на ПКМ
		if (Input.GetMouseButtonDown(1) && grabState == GrabState.Held)
		{
			Throw();
		}
	}

	

	private void FixedUpdate()
	{
		if (heldBody == null || grabState != GrabState.None)
		{
			return; // во время Pulling/Held физически не тянем
		}

		// Target position is a point in front of the camera
		Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;

		// Compute velocity towards target for smooth following
		Vector3 toTarget = targetPosition - heldBody.position;
		Vector3 desiredVelocity = toTarget * 12f;
		heldBody.linearVelocity = Vector3.Lerp(heldBody.linearVelocity, desiredVelocity, 0.5f);
	}


    void Init()
    {
        if (playerCamera == null)
		{
			Debug.LogWarning("PlayerGrab: No camera assigned.");
			return;
		}

		playerController = GetComponent<PlayerController>();
			if (playerController == null)
			{
				Debug.LogError("PlayerController не найден", this);
			}
    }
    private void TryGrab()
	{
		

		Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabbableMask, QueryTriggerInteraction.Ignore))
		{
			Rigidbody rb = hit.rigidbody;
			if (rb == null)
			{
				// Try get from parent if collider on child
				rb = hit.collider.attachedRigidbody;
			}

			if (rb != null && !rb.isKinematic)
			{
				BeginPull(rb);
			}
		}
	}

	// Публичный метод: начать притягивание заранее выбранного Rigidbody (например, клона с Plats)
	public void PickUpRigidbody(Rigidbody rb)
	{
		rb.useGravity = originalUseGravity;
		rb.isKinematic = originalIsKinematic;

		if (rb == null || grabState != GrabState.None) return;
		BeginPull(rb);
	}

	private void BeginPull(Rigidbody rb)
	{
		heldBody = rb;
		grabState = GrabState.Pulling;
		CacheAndApplyHoldPhysics(rb);

		// Проверяем MemSubject и сохраняем тип в memSubjectInHand
		memSubject = rb.GetComponent<MemSubject>();
		if (memSubject != null)
		{
			memSubjectInHand = memSubject.memSubjectType;
		}
		else
		{
			memSubjectInHand = MemSubject.MemSubjectType.None;
		}

		Vector3 targetPos = hand != null ? hand.position : playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
		Quaternion targetRot = hand != null ? hand.rotation : Quaternion.LookRotation(playerCamera.transform.forward, Vector3.up);

		currentTween?.Kill();
		Sequence seq = DOTween.Sequence();
		seq.Join(rb.transform.DOMove(targetPos, pullDuration).SetEase(pullEase));
		if (alignRotationOnAttach)
		{
			seq.Join(rb.transform.DORotateQuaternion(targetRot, pullDuration).SetEase(pullEase));
		}
		seq.OnComplete(() => OnPulledToHand());
		currentTween = seq;
	}

	private void Release()
	{
		if (heldBody == null)
		{
			return;
		}

		// остановить твины
		currentTween?.Kill();
		currentTween = null;

		// отцепить от руки (если был прицеплен)
		if (grabState == GrabState.Held && heldBody.transform.parent == hand)
		{
			heldBody.transform.SetParent(null, true);
		}

		RestorePhysics(heldBody);
		heldBody = null;
		grabState = GrabState.None;

		// Сбрасываем тип в руке при отпускании
		memSubjectInHand = MemSubject.MemSubjectType.None;
	}
	
	private void Throw()
	{
		if (heldBody == null)
		{
			return;
		}
		
		// Останавливаем твины
		currentTween?.Kill();
		currentTween = null;
		
		// Отцепляем от руки
		if (heldBody.transform.parent == hand)
		{
			heldBody.transform.SetParent(null, true);
		}
		
		// Восстанавливаем физику
		RestorePhysics(heldBody);
		
		// Вычисляем направление броска (вперед от камеры)
		Vector3 throwDirection = playerCamera.transform.forward;
		
		// Применяем силу броска
		heldBody.isKinematic = false;
		heldBody.useGravity = true;
		heldBody.linearVelocity = throwDirection * throwForce + Vector3.up * throwUpwardForce;
		
		// Сбрасываем состояние
		heldBody = null;
		grabState = GrabState.None;
		memSubjectInHand = MemSubject.MemSubjectType.None;
	}

	private void OnPulledToHand()
	{
		if (heldBody == null)
		{
			grabState = GrabState.None;
			return;
		}

		if (hand != null)
		{
			heldBody.transform.SetParent(hand, true);
			heldBody.transform.localPosition = Vector3.zero;
			if (alignRotationOnAttach)
			{
				heldBody.transform.localRotation = Quaternion.identity;
			}
		}

		grabState = GrabState.Held;
	}

	private void CacheAndApplyHoldPhysics(Rigidbody rb)
	{
		originalUseGravity = rb.useGravity;
		originalIsKinematic = rb.isKinematic;
		originalDrag = rb.linearDamping;
		originalAngularDrag = rb.angularDamping;
		originalConstraints = rb.constraints;

		// Кэшируем и отключаем коллайдеры
		originalColliders = rb.GetComponentsInChildren<Collider>();
		originalColliderStates = new bool[originalColliders.Length];
		for (int i = 0; i < originalColliders.Length; i++)
		{
			originalColliderStates[i] = originalColliders[i].enabled;
			originalColliders[i].enabled = false;
		}

		rb.useGravity = false;
		rb.linearDamping = heldDrag;
		rb.angularDamping = heldAngularDrag;
		rb.isKinematic = true; // всегда kinematic во время притягивания/удержания
		// фикс вращения оставим свободным; лоджик удержания идёт через родителя
	}

	private void RestorePhysics(Rigidbody rb)
	{
		// Восстанавливаем коллайдеры
		if (originalColliders != null && originalColliderStates != null)
		{
			for (int i = 0; i < originalColliders.Length; i++)
			{
				if (originalColliders[i] != null)
				{
					originalColliders[i].enabled = originalColliderStates[i];
				}
			}
		}

		rb.useGravity = originalUseGravity;
		rb.isKinematic = originalIsKinematic;
		rb.linearDamping = originalDrag;
		rb.angularDamping = originalAngularDrag;
		rb.constraints = originalConstraints;
	}

	private void CheckForPlats()
	{
		Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, platsDetectionDistance, platsLayer, QueryTriggerInteraction.Ignore))
		{
			// Ищем PlatsBehaviour на самом объекте или его родителях
			PlatsBehaviour plats = hit.collider.GetComponentInParent<PlatsBehaviour>();
			if (plats != null)
			{
				// Два кейса:
				// 1) В руке есть предмет → размещаем на Plats (активируем нужный тип) и удаляем предмет из руки
				// 2) Руки пустые → если на Plats есть Subject (подходящий или любой), клонируем и забираем к рукам
				if (heldBody != null && grabState == GrabState.Held)
				{
					// Проверяем, есть ли уже активный объект на Plats
					if (plats.HasActiveSubject())
					{
						Debug.Log($"[PlayerGrab] На платформе '{plats.name}' уже есть активный объект. Нельзя разместить новый.");
						return;
					}
					
					bool placed = plats.ActivateSubjectType(memSubjectInHand);
					if (!placed)
					{
						Debug.Log($"[PlayerGrab] На платформе '{plats.name}' нет подходящего MemSubject для типа {memSubjectInHand}");
						return;
					}

					// Успешно разместили → удаляем предмет из руки
					currentTween?.Kill();
					if (heldBody.transform.parent == hand)
					{
						heldBody.transform.SetParent(null, true);
					}
					Destroy(heldBody.gameObject);
					heldBody = null;
					grabState = GrabState.None;
					memSubjectInHand = MemSubject.MemSubjectType.None;
				}
				else if (heldBody == null && grabState == GrabState.None)
				{
					// Руки пустые → пробуем склонировать с Plats и сразу забрать
					if (plats.TryCloneSubject(memSubjectInHand, out Rigidbody cloneRb))
					{
						
						PickUpRigidbody(cloneRb);

					}
				}
			}
		}
	}
}


