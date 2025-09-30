using UnityEngine;


public class PlatformAttachTrigger : MonoBehaviour
{
	[SerializeField] private bool debugLogs = true;
	[SerializeField] private bool autoAddTriggers = true; // Включить/выключить автоматическое добавление триггеров

	
	private const string PlayerLayerName = "Player";

	
	private void Awake()
	{
		// Гарантируем наличие isTrigger-коллайдера
		Collider ownCollider = GetComponent<Collider>();
		if (ownCollider == null)
		{
			ownCollider = gameObject.AddComponent<BoxCollider>();
			if (debugLogs) Debug.Log("[PlatformAttachTrigger] Добавлен BoxCollider (коллайдера не было)", this);
		}
		if (!ownCollider.isTrigger)
		{
			ownCollider.isTrigger = true;
			if (debugLogs) Debug.Log("[PlatformAttachTrigger] Коллайдер помечен как isTrigger", this);
		}

		// Риджидбоди (кинематический) для надёжного получения событий триггера
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.isKinematic = true;
			
		}

		// Добавляем триггеры к дочерним объектам, если включена опция
		if (autoAddTriggers)
		{
			AddTriggersToPlayerLayerChildren();
		}
	}	

	private void OnTriggerEnter(Collider other)
	{
		// Берём непосредственно объект, который вошёл в триггер
		if (other.gameObject.layer == LayerMask.NameToLayer(PlayerLayerName))
		{
			Transform t = other.transform;
			if (t.parent != transform)
			{
				t.SetParent(transform, true);
				if (debugLogs)
					Debug.Log($"[PlatformAttachTrigger] {t.name} сделан дочерним {transform.name}", this);
			}
		}
	}
	private void OnTriggerExit(Collider other)
	{
		// Берём непосредственно объект, покинувший триггер
		if (other.gameObject.layer == LayerMask.NameToLayer(PlayerLayerName))
		{
			Transform t = other.transform;
			if (t.parent == transform)
			{
				t.SetParent(null, true);
				if (debugLogs)
					Debug.Log($"[PlatformAttachTrigger] {t.name} отсоединён от {transform.name} (OnTriggerExit)", this);
			}
		}
	}

	private void AddTriggersToPlayerLayerChildren()
	{
		// Получаем индекс слоя Player
		int playerLayer = LayerMask.NameToLayer(PlayerLayerName);
		if (playerLayer == -1)
		{
			if (debugLogs)
				Debug.LogWarning("[PlatformAttachTrigger] Слой Player не найден!", this);
			return;
		}

		// Проходим по всем дочерним объектам
		foreach (Transform child in transform)
		{
			// Проверяем, находится ли объект на слое Player
			if (child.gameObject.layer == playerLayer)
			{
				// Проверяем, есть ли у объекта коллайдер
				Collider collider = child.GetComponent<Collider>();
				if (collider == null)
				{
					// Добавляем коллайдер, если его нет
					collider = child.gameObject.AddComponent<BoxCollider>();
					if (debugLogs)
						Debug.Log($"[PlatformAttachTrigger] Добавлен BoxCollider к {child.name}", this);
				}

				// Устанавливаем коллайдер как триггер
				if (!collider.isTrigger)
				{
					collider.isTrigger = true;
					if (debugLogs)
						Debug.Log($"[PlatformAttachTrigger] Коллайдер у {child.name} установлен как триггер", this);
				}

				// Добавляем скрипт PlatformAttachTrigger, если его нет
				if (child.GetComponent<PlatformAttachTrigger>() == null)
				{
					child.gameObject.AddComponent<PlatformAttachTrigger>();
					if (debugLogs)
						Debug.Log($"[PlatformAttachTrigger] Добавлен скрипт PlatformAttachTrigger к {child.name}", this);
				}
			}
		}
	}
}