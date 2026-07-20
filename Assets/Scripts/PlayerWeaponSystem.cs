using UnityEngine;

public class PlayerWeaponSystem : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject heldWeaponObject;
    [SerializeField] private GameObject crosshairObject;

    [Header("Shooting Settings")]
    [SerializeField] private float shootingRange = 20f;
    [SerializeField] private float shootingCooldown = 0.5f;
    [SerializeField] private float pigStunDuration = 3f;
    [SerializeField] private LayerMask hitMask = ~0;

    public bool HasWeapon { get; private set; }

    private float nextShootingTime;
    private bool lastCrosshairState;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        HasWeapon = false;

        if (heldWeaponObject != null)
        {
            heldWeaponObject.SetActive(false);
        }

        SetCrosshairVisible(false);
    }

    private void Update()
    {
        bool canUseWeapon =
            HasWeapon &&
            !PlayerHideState.IsHidden &&
            Time.timeScale > 0f;

        // 没有武器、正在躲藏或Game Over时隐藏准星
        SetCrosshairVisible(canUseWeapon);

        if (!canUseWeapon)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) &&
            Time.time >= nextShootingTime)
        {
            nextShootingTime =
                Time.time + shootingCooldown;

            Shoot();
        }
    }

    public void EquipWeapon()
    {
        if (HasWeapon)
        {
            return;
        }

        HasWeapon = true;

        if (heldWeaponObject != null)
        {
            heldWeaponObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "PlayerWeaponSystem: Held Weapon Object is not assigned."
            );
        }

        SetCrosshairVisible(true);

        Debug.Log("Player picked up the weapon.");
    }

    private void Shoot()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning(
                "PlayerWeaponSystem: Player Camera is not assigned."
            );
            return;
        }

        // 射线从屏幕中央，也就是准星位置发出
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                shootingRange,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Shot hit: " + hit.collider.name);

            EnemyPigStun pigStun =
                hit.collider.GetComponentInParent<EnemyPigStun>();

            if (pigStun != null)
            {
                pigStun.Stun(pigStunDuration);
            }
        }
        else
        {
            Debug.Log("Shot missed.");
        }

        Debug.DrawRay(
            ray.origin,
            ray.direction * shootingRange,
            Color.red,
            1f
        );
    }

    private void SetCrosshairVisible(bool visible)
    {
        if (crosshairObject == null)
        {
            return;
        }

        // 避免每一帧重复设置相同状态
        if (lastCrosshairState == visible &&
            crosshairObject.activeSelf == visible)
        {
            return;
        }

        crosshairObject.SetActive(visible);
        lastCrosshairState = visible;
    }

    private void OnDisable()
    {
        SetCrosshairVisible(false);
    }
}