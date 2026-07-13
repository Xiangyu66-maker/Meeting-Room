using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 谜题管理器：监听物品拾取/放置事件，更新谜题状态，触发机关。
/// 挂载到场景中的空物体上（如 "_PuzzleManager"）。
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Header("谜题配置")]
    [SerializeField] private string[] requiredItemsForDoor = { "document_01", "remote_01" }; // 集齐后开门
    [SerializeField] private string remoteId = "remote_01";       // 遥控器ID
    [SerializeField] private string screenId = "screen_01";       // 屏幕ID
    [SerializeField] private string doorId = "locked_door_01";    // 门ID

    // 状态记录
    private HashSet<string> collectedItems = new HashSet<string>();
    private bool doorUnlocked = false;
    private bool screenActivated = false;

    // 缓存引用
    private DoorController doorController;
    private GameObject screenObject;

    private void Start()
    {
        // 查找场景中的门和屏幕
        doorController = FindDoor();
        screenObject = FindScreen();

        // 订阅事件
        if (PuzzleEventManager.Instance != null)
        {
            PuzzleEventManager.Instance.OnItemGrabbed += OnItemGrabbedHandler;
            PuzzleEventManager.Instance.OnItemDropped += OnItemDroppedHandler;
        }
        else
        {
            Debug.LogWarning("PuzzleEventManager 未找到，请确保场景中有 PuzzleEventManager 组件。");
        }
    }

    private void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        if (PuzzleEventManager.Instance != null)
        {
            PuzzleEventManager.Instance.OnItemGrabbed -= OnItemGrabbedHandler;
            PuzzleEventManager.Instance.OnItemDropped -= OnItemDroppedHandler;
        }
    }

    // ---------- 事件处理器 ----------
    private void OnItemGrabbedHandler(string objectId)
    {
        Debug.Log($"拾取事件: {objectId}");

        // 记录拾取（所有物品都记录，但只有配置的才用于检查）
        collectedItems.Add(objectId);

        // 1. 单步触发：捡到遥控器 → 激活屏幕（但这里我们改由“放置”触发，所以注释掉）
        // 如果想改为拾取即触发，取消注释：
        // if (objectId == remoteId) ActivateScreen();

        // 2. 多重收集：检查集齐物品
        CheckDoorCollection();
    }

    private void OnItemDroppedHandler(string objectId, GameObject surface)
    {
        if (surface == null) return;

        string surfaceId = surface.GetComponent<ObjectIdentity>()?.ObjectId;
        Debug.Log($"放置事件: {objectId} 放在了 {(surfaceId ?? surface.name)} 上");

        // 3. 放置机关：把遥控器放在屏幕上 → 激活屏幕
        if (objectId == remoteId && surfaceId == screenId)
        {
            ActivateScreen();
        }

        // 可以添加更多放置规则，例如把文档放在桌子上触发提示等
    }

    // ---------- 谜题逻辑函数 ----------
    private void CheckDoorCollection()
    {
        if (doorUnlocked) return; // 已解锁不再重复

        bool allCollected = true;
        foreach (string id in requiredItemsForDoor)
        {
            if (!collectedItems.Contains(id))
            {
                allCollected = false;
                break;
            }
        }

        if (allCollected)
        {
            UnlockDoor();
        }
    }

    private void UnlockDoor()
    {
        if (doorController != null)
        {
            doorController.UnlockDoor();
            doorUnlocked = true;
            Debug.Log("门已解锁！（通过集齐物品）");
        }
        else
        {
            Debug.LogWarning("门控制器未找到，无法解锁。");
        }
    }

    private void ActivateScreen()
    {
        if (screenActivated) return;

        if (screenObject != null)
        {
            // 示例：改变屏幕颜色，或显示密码文字
            Renderer r = screenObject.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = Color.green;
                Debug.Log("屏幕已激活（绿色）");
            }

            // 如果屏幕上有 TextMeshPro 或 UI 文字，可以显示密码
            // 例如：screenObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "密码: 3142";
        }
        else
        {
            Debug.LogWarning("屏幕对象未找到，无法激活。");
        }

        screenActivated = true;
        // 触发其他效果（如成就、音效等）
    }

    // ---------- 辅助查找方法 ----------
    private DoorController FindDoor()
    {
        // 优先通过 ObjectIdentity 查找
        ObjectIdentity[] identities = FindObjectsOfType<ObjectIdentity>();
        foreach (var id in identities)
        {
            if (id.ObjectId == doorId)
            {
                DoorController door = id.GetComponent<DoorController>();
                if (door != null) return door;
            }
        }
        // 兜底：直接按类型查找
        return FindObjectOfType<DoorController>();
    }

    private GameObject FindScreen()
    {
        ObjectIdentity[] identities = FindObjectsOfType<ObjectIdentity>();
        foreach (var id in identities)
        {
            if (id.ObjectId == screenId)
                return id.gameObject;
        }
        // 兜底按名称查找
        return GameObject.Find(screenId);
    }
}