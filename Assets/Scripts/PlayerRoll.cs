using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRoll : MonoBehaviour
{
    [Header("References")]
    [Tooltip("玩家当前使用的第一人称移动控制脚本")]
    [SerializeField] private Behaviour movementController;

    [Tooltip("玩家的第一人称摄像机")]
    [SerializeField] private Transform playerCamera;

    [Header("Roll Settings")]
    [Tooltip("翻滚按键")]
    [SerializeField] private KeyCode rollKey = KeyCode.LeftControl;

    [Tooltip("翻滚移动速度")]
    [SerializeField] private float rollSpeed = 5f;

    [Tooltip("一次翻滚持续时间")]
    [SerializeField] private float rollDuration = 0.45f;

    [Tooltip("两次翻滚之间的冷却时间")]
    [SerializeField] private float rollCooldown = 1.2f;

    [Tooltip("没有按WASD时，默认向前翻滚")]
    [SerializeField] private bool rollForwardWithoutInput = true;

    [Header("Camera Effect")]
    [Tooltip("翻滚时镜头倾斜角度")]
    [SerializeField] private float cameraTiltAngle = 10f;

    [Tooltip("是否启用镜头倾斜")]
    [SerializeField] private bool useCameraTilt = true;

    public bool IsRolling { get; private set; }

    private Rigidbody playerRigidbody;

    private float nextRollTime;
    private float lastGroundedTime;

    private Quaternion normalCameraRotation;

    private bool movementControllerWasEnabled;
    private Coroutine rollCoroutine;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        if (playerCamera == null)
        {
            Camera foundCamera =
                GetComponentInChildren<Camera>(true);

            if (foundCamera != null)
            {
                playerCamera = foundCamera.transform;
            }
        }

        if (playerCamera != null)
        {
            normalCameraRotation =
                playerCamera.localRotation;
        }

        IsRolling = false;
        nextRollTime = 0f;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (PlayerHideState.IsHidden)
        {
            return;
        }

        bool pressedRollKey =
            Input.GetKeyDown(rollKey) ||
            Input.GetKeyDown(KeyCode.RightControl);

        if (pressedRollKey)
        {
            TryStartRoll();
        }
    }

    private void TryStartRoll()
    {
        if (IsRolling)
        {
            return;
        }

        if (Time.time < nextRollTime)
        {
            return;
        }

        if (!IsGrounded())
        {
            Debug.Log("Cannot roll while in the air.");
            return;
        }

        Vector3 rollDirection =
            GetRollDirection();

        if (rollDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        rollCoroutine = StartCoroutine(
            RollRoutine(rollDirection)
        );
    }

    private Vector3 GetRollDirection()
    {
        float horizontalInput =
            Input.GetAxisRaw("Horizontal");

        float verticalInput =
            Input.GetAxisRaw("Vertical");

        Transform directionReference =
            playerCamera != null
                ? playerCamera
                : transform;

        Vector3 forward =
            directionReference.forward;

        Vector3 right =
            directionReference.right;

        // 翻滚只在地面水平方向移动
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction =
            forward * verticalInput +
            right * horizontalInput;

        /*
         * 玩家没有按WASD时，
         * 按Ctrl默认向视角前方翻滚。
         */
        if (direction.sqrMagnitude < 0.001f &&
            rollForwardWithoutInput)
        {
            direction = forward;
        }

        return direction.normalized;
    }

    private IEnumerator RollRoutine(
        Vector3 rollDirection)
    {
        IsRolling = true;

        nextRollTime =
            Time.time + rollCooldown;

        /*
         * 临时关闭原来的第一人称控制脚本，
         * 防止它和翻滚速度互相冲突。
         */
        if (movementController != null)
        {
            movementControllerWasEnabled =
                movementController.enabled;

            movementController.enabled = false;
        }

        if (playerCamera != null)
        {
            normalCameraRotation =
                playerCamera.localRotation;
        }

        float elapsedTime = 0f;

        while (elapsedTime < rollDuration)
        {
            elapsedTime += Time.fixedDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / rollDuration
                );

            /*
             * 翻滚刚开始速度最快，
             * 快结束时逐渐减速。
             */
            float currentSpeed =
                Mathf.Lerp(
                    rollSpeed,
                    rollSpeed * 0.35f,
                    progress
                );

            Vector3 currentVelocity =
                playerRigidbody.linearVelocity;

            Vector3 rollVelocity =
                rollDirection * currentSpeed;

            /*
             * 只替换水平速度，
             * 保留原来的垂直速度和重力。
             */
            playerRigidbody.linearVelocity =
                new Vector3(
                    rollVelocity.x,
                    currentVelocity.y,
                    rollVelocity.z
                );

            UpdateCameraTilt(progress);

            yield return new WaitForFixedUpdate();
        }

        FinishRoll();
    }

    private void UpdateCameraTilt(float progress)
    {
        if (!useCameraTilt ||
            playerCamera == null)
        {
            return;
        }

        /*
         * 镜头先向一侧倾斜，
         * 然后在翻滚结束前恢复。
         */
        float tiltProgress =
            Mathf.Sin(progress * Mathf.PI);

        float tilt =
            cameraTiltAngle * tiltProgress;

        Quaternion tiltedRotation =
            normalCameraRotation *
            Quaternion.Euler(0f, 0f, -tilt);

        playerCamera.localRotation =
            Quaternion.Slerp(
                playerCamera.localRotation,
                tiltedRotation,
                0.35f
            );
    }

    private void FinishRoll()
    {
        Vector3 currentVelocity =
            playerRigidbody.linearVelocity;

        /*
         * 翻滚结束后降低水平惯性，
         * 防止玩家继续向前滑很远。
         */
        playerRigidbody.linearVelocity =
            new Vector3(
                currentVelocity.x * 0.25f,
                currentVelocity.y,
                currentVelocity.z * 0.25f
            );

        if (playerCamera != null)
        {
            playerCamera.localRotation =
                normalCameraRotation;
        }

        if (movementController != null)
        {
            movementController.enabled =
                movementControllerWasEnabled;
        }

        IsRolling = false;
        rollCoroutine = null;

        Debug.Log("Player roll finished.");
    }

    private bool IsGrounded()
    {
        /*
         * OnCollisionStay每帧更新最后接触地面的时间。
         * 保留0.15秒容错，避免台阶处无法翻滚。
         */
        return Time.time - lastGroundedTime < 0.15f;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            /*
             * 法线朝上，说明玩家脚下接触的是地面。
             */
            if (contact.normal.y > 0.5f)
            {
                lastGroundedTime = Time.time;
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
            rollCoroutine = null;
        }

        if (movementController != null)
        {
            movementController.enabled =
                movementControllerWasEnabled;
        }

        if (playerCamera != null)
        {
            playerCamera.localRotation =
                normalCameraRotation;
        }

        IsRolling = false;
    }
}
