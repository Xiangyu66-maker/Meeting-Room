using UnityEngine;
using UnityEngine.AI;

public class EnemyPigChase : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Chase Settings")]
    [Tooltip("猪追逐玩家时的初始速度")]
    [SerializeField] private float chaseSpeed = 0.8f;

    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Patrol Settings")]
    [Tooltip("玩家躲藏时，猪的巡逻速度")]
    [SerializeField] private float patrolSpeed = 0.5f;

    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float patrolWaitTime = 2f;

    private NavMeshAgent agent;
    private float patrolTimer;
    private bool wasHiddenLastFrame;

    public float CurrentChaseSpeed
    {
        get { return chaseSpeed; }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                "EnemyPigChase: No NavMeshAgent found."
            );

            enabled = false;
            return;
        }

        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError(
                    "EnemyPigChase: Cannot find Player tag."
                );
            }
        }
    }

    private void Update()
    {
        /*
         * 眩晕脚本可能暂时关闭NavMeshAgent，
         * 所以要先检查Agent是否可用。
         */
        if (agent == null || !agent.enabled)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        if (PlayerHideState.IsHidden)
        {
            if (!wasHiddenLastFrame)
            {
                OnPlayerStartedHiding();
            }

            PatrolRandomly();
        }
        else
        {
            if (wasHiddenLastFrame)
            {
                OnPlayerStoppedHiding();
            }

            ChasePlayer();
        }

        wasHiddenLastFrame =
            PlayerHideState.IsHidden;
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            return;
        }

        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(player.position);
    }

    private void PatrolRandomly()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.2f;

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                SetRandomPatrolDestination();
                patrolTimer = 0f;
            }
        }
    }

    private void OnPlayerStartedHiding()
    {
        Debug.Log(
            "Player is hidden. Pig starts patrolling."
        );

        agent.ResetPath();
        patrolTimer = patrolWaitTime;

        SetRandomPatrolDestination();
    }

    private void OnPlayerStoppedHiding()
    {
        Debug.Log(
            "Player came out. Pig resumes chasing."
        );

        agent.ResetPath();
        patrolTimer = 0f;
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere * patrolRadius;

        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(
                randomDirection,
                out NavMeshHit hit,
                patrolRadius,
                NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    /// <summary>
    /// EnemyPigStun调用此方法修改追逐速度。
    /// </summary>
    public void SetChaseSpeed(float newSpeed)
    {
        chaseSpeed = Mathf.Max(0f, newSpeed);

        /*
         * 如果当前正在追逐，立即更新。
         * 如果玩家正在躲藏，继续使用Patrol Speed。
         */
        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh &&
            !PlayerHideState.IsHidden)
        {
            agent.speed = chaseSpeed;
        }

        Debug.Log(
            "Pig chase speed changed to: " +
            chaseSpeed.ToString("F1")
        );
    }
}