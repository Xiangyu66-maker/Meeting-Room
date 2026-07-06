using UnityEngine;
using UnityEngine.AI;

public class EnemyPigChase : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float stoppingDistance = 0.2f;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("EnemyPigChase: No NavMeshAgent found on this object.");
            return;
        }

        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("EnemyPigChase: Cannot find object with tag Player.");
            }
        }
    }

    private void Update()
    {
        if (agent == null || player == null)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("EnemyPigChase: EnemyPig is not on NavMesh.");
            return;
        }

        agent.SetDestination(player.position);
    }
}