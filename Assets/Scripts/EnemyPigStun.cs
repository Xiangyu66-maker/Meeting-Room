using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPigStun : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("中枪后后退的距离")]
    [SerializeField] private float knockbackDistance = 2f;

    [Tooltip("击退运动持续时间")]
    [SerializeField] private float knockbackDuration = 0.25f;

    [Tooltip("击退时轻微腾空的高度")]
    [SerializeField] private float knockbackHeight = 0.25f;

    [Header("Rage Settings")]
    [Tooltip("每次中枪后增加的追逐速度")]
    [SerializeField] private float speedIncreasePerHit = 0.1f;

    [Tooltip("猪最终能够达到的最大追逐速度")]
    [SerializeField] private float maximumSpeed = 1.5f;

    private EnemyPigChase chaseScript;
    private EnemyPigCatchPlayer catchScript;
    private PigRageVisual rageVisual;

    private NavMeshAgent agent;
    private Animator pigAnimator;

    private Coroutine stunCoroutine;
    private bool isStunned;

    private bool chaseWasEnabled;
    private bool catchWasEnabled;
    private bool agentWasEnabled;
    private bool agentWasStopped;

    private float previousAnimatorSpeed = 1f;

    private float startingSpeed;
    private float currentRageSpeed;
    private int rageLevel;

    public int RageLevel
    {
        get { return rageLevel; }
    }

    public float CurrentSpeed
    {
        get { return currentRageSpeed; }
    }

    private void Awake()
    {
        chaseScript =
            GetComponent<EnemyPigChase>();

        catchScript =
            GetComponent<EnemyPigCatchPlayer>();

        rageVisual =
            GetComponent<PigRageVisual>();

        agent =
            GetComponent<NavMeshAgent>();

        pigAnimator =
            GetComponentInChildren<Animator>();

        /*
         * 从EnemyPigChase读取当前初始速度。
         */
        if (chaseScript != null)
        {
            startingSpeed =
                chaseScript.CurrentChaseSpeed;
        }
        else if (agent != null)
        {
            startingSpeed =
                agent.speed;
        }
        else
        {
            startingSpeed = 0.8f;
        }

        currentRageSpeed =
            startingSpeed;

        rageLevel = 0;

        if (pigAnimator != null)
        {
            previousAnimatorSpeed =
                pigAnimator.speed;
        }

        if (rageVisual != null)
        {
            rageVisual.SetRageAmount(0f);
        }
    }

    /// <summary>
    /// 玩家武器击中猪时调用。
    /// duration = 眩晕时间
    /// shotDirection = 子弹飞行方向
    /// </summary>
    public void Stun(
        float duration,
        Vector3 shotDirection)
    {
        if (duration <= 0f)
        {
            return;
        }

        /*
         * 每次中枪都会提高速度并增加红色程度。
         */
        IncreaseRage();

        /*
         * 只有第一次进入眩晕时，
         * 才保存猪之前的组件状态。
         */
        if (!isStunned)
        {
            chaseWasEnabled =
                chaseScript != null &&
                chaseScript.enabled;

            catchWasEnabled =
                catchScript != null &&
                catchScript.enabled;

            if (agent != null)
            {
                agentWasEnabled =
                    agent.enabled;

                if (agent.enabled &&
                    agent.isOnNavMesh)
                {
                    agentWasStopped =
                        agent.isStopped;
                }
            }

            if (pigAnimator != null)
            {
                previousAnimatorSpeed =
                    pigAnimator.speed;
            }
        }

        /*
         * 眩晕期间再次中枪：
         * 重新开始完整的眩晕计时。
         */
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(
            StunRoutine(
                duration,
                shotDirection
            )
        );
    }

    private void IncreaseRage()
    {
        rageLevel++;

        currentRageSpeed = Mathf.Min(
            startingSpeed +
            rageLevel * speedIncreasePerHit,
            maximumSpeed
        );

        /*
         * 更新EnemyPigChase保存的追逐速度。
         */
        if (chaseScript != null)
        {
            chaseScript.SetChaseSpeed(
                currentRageSpeed
            );
        }

        /*
         * 把当前速度换算成0～1的红色程度。
         */
        float rageAmount =
            Mathf.InverseLerp(
                startingSpeed,
                maximumSpeed,
                currentRageSpeed
            );

        if (rageVisual != null)
        {
            rageVisual.SetRageAmount(
                rageAmount
            );
        }

        Debug.Log(
            "Pig Rage Level: " +
            rageLevel +
            " | Speed: " +
            currentRageSpeed.ToString("F1") +
            " | Red Amount: " +
            rageAmount.ToString("F2")
        );
    }

    private IEnumerator StunRoutine(
        float duration,
        Vector3 shotDirection)
    {
        isStunned = true;

        /*
         * 停止追逐和抓人。
         */
        if (chaseScript != null)
        {
            chaseScript.enabled = false;
        }

        if (catchScript != null)
        {
            catchScript.enabled = false;
        }

        /*
         * 暂停动画。
         */
        if (pigAnimator != null)
        {
            pigAnimator.speed = 0f;
        }

        /*
         * 停止并关闭NavMeshAgent，
         * 让脚本手动控制击退位移。
         */
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            agent.enabled = false;
        }

        /*
         * 只保留水平方向。
         */
        Vector3 knockbackDirection =
            shotDirection;

        knockbackDirection.y = 0f;

        if (knockbackDirection.sqrMagnitude <
            0.001f)
        {
            knockbackDirection =
                -transform.forward;
        }

        knockbackDirection.Normalize();

        Vector3 startPosition =
            transform.position;

        Vector3 wantedPosition =
            startPosition +
            knockbackDirection *
            knockbackDistance;

        Vector3 targetPosition =
            wantedPosition;

        /*
         * 防止击退到NavMesh以外或穿过墙壁。
         */
        if (NavMesh.Raycast(
                startPosition,
                wantedPosition,
                out NavMeshHit blockingHit,
                NavMesh.AllAreas))
        {
            targetPosition =
                blockingHit.position -
                knockbackDirection * 0.15f;
        }

        float elapsedTime = 0f;

        while (elapsedTime <
               knockbackDuration)
        {
            elapsedTime +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    knockbackDuration
                );

            /*
             * 开始较快，结束时逐渐减慢。
             */
            float smoothProgress =
                1f -
                Mathf.Pow(
                    1f - progress,
                    3f
                );

            Vector3 newPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );

            /*
             * 轻微腾空弧线。
             */
            newPosition.y +=
                Mathf.Sin(
                    progress *
                    Mathf.PI
                ) *
                knockbackHeight;

            transform.position =
                newPosition;

            yield return null;
        }

        /*
         * 击退结束后落回最近的NavMesh。
         */
        if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit landingHit,
                2f,
                NavMesh.AllAreas))
        {
            transform.position =
                landingHit.position;
        }
        else
        {
            transform.position =
                targetPosition;
        }

        /*
         * 击退时间包含在总眩晕时间中。
         */
        float remainingStunTime =
            Mathf.Max(
                0f,
                duration -
                knockbackDuration
            );

        yield return new WaitForSeconds(
            remainingStunTime
        );

        RestorePig();

        isStunned = false;
        stunCoroutine = null;

        Debug.Log(
            "Pig recovered. Chase speed: " +
            currentRageSpeed.ToString("F1")
        );
    }

    private void RestorePig()
    {
        /*
         * 先恢复NavMeshAgent。
         */
        if (agent != null &&
            agentWasEnabled)
        {
            if (NavMesh.SamplePosition(
                    transform.position,
                    out NavMeshHit nearestHit,
                    2f,
                    NavMesh.AllAreas))
            {
                transform.position =
                    nearestHit.position;
            }

            agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.Warp(
                    transform.position
                );

                agent.speed =
                    currentRageSpeed;

                agent.isStopped =
                    agentWasStopped;
            }
        }

        /*
         * 再恢复追逐和抓人脚本。
         */
        if (chaseScript != null)
        {
            chaseScript.enabled =
                chaseWasEnabled;
        }

        if (catchScript != null)
        {
            catchScript.enabled =
                catchWasEnabled;
        }

        /*
         * 恢复动画。
         */
        if (pigAnimator != null)
        {
            pigAnimator.speed =
                previousAnimatorSpeed;
        }
    }
}