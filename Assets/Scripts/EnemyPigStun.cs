using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPigStun : MonoBehaviour
{
    private EnemyPigChase chaseScript;
    private EnemyPigCatchPlayer catchScript;
    private NavMeshAgent agent;
    private Animator pigAnimator;

    private Coroutine stunCoroutine;
    private bool isStunned;

    private bool chaseWasEnabled;
    private bool catchWasEnabled;
    private bool agentWasStopped;
    private float previousAnimatorSpeed = 1f;

    private void Awake()
    {
        chaseScript = GetComponent<EnemyPigChase>();
        catchScript = GetComponent<EnemyPigCatchPlayer>();
        agent = GetComponent<NavMeshAgent>();
        pigAnimator = GetComponentInChildren<Animator>();

        if (pigAnimator != null)
        {
            previousAnimatorSpeed = pigAnimator.speed;
        }
    }

    public void Stun(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        // 只有第一次进入眩晕时才记录原始状态
        if (!isStunned)
        {
            chaseWasEnabled =
                chaseScript != null && chaseScript.enabled;

            catchWasEnabled =
                catchScript != null && catchScript.enabled;

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agentWasStopped = agent.isStopped;
            }

            if (pigAnimator != null)
            {
                previousAnimatorSpeed = pigAnimator.speed;
            }
        }

        // 连续中枪会重新开始计算3秒
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // 停止追踪
        if (chaseScript != null)
        {
            chaseScript.enabled = false;
        }

        // 眩晕时不能抓到玩家
        if (catchScript != null)
        {
            catchScript.enabled = false;
        }

        // 停止NavMesh移动
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 暂停走路动画，避免原地踏步
        if (pigAnimator != null)
        {
            pigAnimator.speed = 0f;
        }

        Debug.Log("Pig stunned for " + duration + " seconds.");

        yield return new WaitForSeconds(duration);

        // 恢复NavMesh
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = agentWasStopped;
        }

        // 恢复脚本
        if (chaseScript != null)
        {
            chaseScript.enabled = chaseWasEnabled;
        }

        if (catchScript != null)
        {
            catchScript.enabled = catchWasEnabled;
        }

        // 恢复动画
        if (pigAnimator != null)
        {
            pigAnimator.speed = previousAnimatorSpeed;
        }

        isStunned = false;
        stunCoroutine = null;

        Debug.Log("Pig recovered from stun.");
    }
}