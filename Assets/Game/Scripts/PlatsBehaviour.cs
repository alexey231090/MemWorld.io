using UnityEngine;

public class PlatsBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerGrab playerGrab;

    void Start()
    {      
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init()
    {
        playerGrab = FindFirstObjectByType<PlayerGrab>();
        if (playerGrab == null)
        {
            Debug.LogError("[PlatsBehaviour] PlayerGrab not found in scene", this);
        }
    }

	

	// Включает дочерний объект с MemSubject, чей тип совпадает с переданным
	public bool ActivateSubjectType(MemSubject.MemSubjectType typeToActivate)
	{
		if (typeToActivate == MemSubject.MemSubjectType.None)
		{
			return false;
		}

		MemSubject[] subjects = GetComponentsInChildren<MemSubject>(includeInactive: true);

		// Сначала отключаем все subject
		for (int i = 0; i < subjects.Length; i++)
		{
			if (subjects[i] != null)
			{
				subjects[i].gameObject.SetActive(false);
			}
		}

		// Затем включаем только нужный тип
		bool anyActivated = false;
		for (int i = 0; i < subjects.Length; i++)
		{
			MemSubject subject = subjects[i];
			if (subject != null && subject.memSubjectType == typeToActivate)
			{
				subject.gameObject.SetActive(true);
				anyActivated = true;
			}
		}

		return anyActivated;
	}
	
	// Проверяет, есть ли активный объект на платформе
	public bool HasActiveSubject()
	{
		MemSubject[] subjects = GetComponentsInChildren<MemSubject>(includeInactive: false);
		return subjects != null && subjects.Length > 0;
	}

	// Клонирует первый подходящий активный MemSubject (по типу, либо любой если typeFilter == None),
	// размещает клон в том же мировом положении/повороте, отсоединённым от Plats, отключает оригинал.
	// Возвращает true и Rigidbody клона, если удалось.
	public bool TryCloneSubject(MemSubject.MemSubjectType typeFilter, out Rigidbody cloneRigidbody)
	{
		cloneRigidbody = null;
		MemSubject[] subjects = GetComponentsInChildren<MemSubject>(includeInactive: false);
		if (subjects == null || subjects.Length == 0)
		{
			return false;
		}

		for (int i = 0; i < subjects.Length; i++)
		{
			MemSubject subject = subjects[i];
			if (subject == null) continue;
			if (typeFilter != MemSubject.MemSubjectType.None && subject.memSubjectType != typeFilter) continue;

			Transform src = subject.transform;
			GameObject clone = Instantiate(subject.gameObject);
			clone.transform.SetParent(null, true);
			clone.transform.position = src.position;
			clone.transform.rotation = src.rotation;
			// Пробуем сохранить визуальный размер максимально близко
			clone.transform.localScale = subject.transform.localScale;

			// Устанавливаем слой Grab для клона (и всех его детей), если слой существует
			int grabLayer = LayerMask.NameToLayer("Grab");
			if (grabLayer != -1)
			{
				SetLayerRecursively(clone, grabLayer);
			}

			// Отключаем оригинал
			subject.gameObject.SetActive(false);

			// Удаляем Shooter компонент с клона (чтобы не стрелял)
			Shooter shooter = clone.GetComponent<Shooter>();
			if (shooter != null)
			{
				DestroyImmediate(shooter);
			}

			// Гарантируем наличие Rigidbody у клона (для притягивания)
			cloneRigidbody = clone.GetComponent<Rigidbody>();
			if (cloneRigidbody == null)
			{
				cloneRigidbody = clone.AddComponent<Rigidbody>();
				cloneRigidbody.useGravity = false;
				cloneRigidbody.isKinematic = false;
			}

			return true;
		}

		return false;
	}

	private void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
		}
	}
}
