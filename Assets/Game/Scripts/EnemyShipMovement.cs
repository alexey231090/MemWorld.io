using UnityEngine;
using UnityEngine.AI;

public class EnemyShipMovement : MonoBehaviour
{
	[Header("Targeting")]
	[SerializeField] private Transform target; // за кем следить (корабль игрока)
	[SerializeField] private float activationRadius = 40f; // радиус активации слежения
	[SerializeField] private float stoppingDistance = 5f; // дистанция остановки у цели
	[SerializeField] private float repathInterval = 0.25f; // период обновления пути

	[Header("Agent Settings")]
	[SerializeField] private float moveSpeed = 6f;
	[SerializeField] private float acceleration = 12f;
	[SerializeField] private float angularSpeed = 120f;

	private NavMeshAgent agent;
	private float nextRepathTime;

	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		if (agent == null)
		{
			Debug.LogError("[EnemyShipMovement] NavMeshAgent not found on GameObject", this);
			return;
		}

		agent.stoppingDistance = stoppingDistance;
		agent.speed = moveSpeed;
		agent.acceleration = acceleration;
		agent.angularSpeed = angularSpeed;
		agent.updateRotation = true;
	}

	void Update()
	{
		if (agent == null) return;

		// Если нет цели — стоим
		if (target == null)
		{
			if (!agent.isStopped) agent.isStopped = true;
			return;
		}

		// Проверка активационного радиуса
		float distanceToTarget = Vector3.Distance(transform.position, target.position);
		bool shouldChase = distanceToTarget <= activationRadius;

		if (!shouldChase)
		{
			if (!agent.isStopped) agent.isStopped = true;
			return;
		}

		// В пределах радиуса — преследуем по навмешу
		if (agent.isStopped) agent.isStopped = false;

		// Обновляем путь с заданной периодичностью
		if (Time.time >= nextRepathTime)
		{
			agent.stoppingDistance = stoppingDistance;
			agent.speed = moveSpeed;
			agent.acceleration = acceleration;
			agent.angularSpeed = angularSpeed;
			agent.SetDestination(target.position);
			nextRepathTime = Time.time + repathInterval;
		}
	}

	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.25f);
		Gizmos.DrawSphere(transform.position, activationRadius);
		Gizmos.color = new Color(1f, 0.8f, 0.1f, 1f);
		Gizmos.DrawWireSphere(transform.position, activationRadius);
	}
}
