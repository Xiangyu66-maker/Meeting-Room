using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("谜题配置")]
    [SerializeField] private string[] requiredItemsForDoor = { "document_01", "remote_01" };
    [SerializeField] private string remoteId = "remote_01";
    [SerializeField] private string screenId = "screen_01";
    [SerializeField] private string doorId = "locked_door_01";

    [Header("座椅配置")]
    [SerializeField] private string seatIdPrefix = "chair_";

    private HashSet<string> collectedItems = new HashSet<string>();
    private bool doorUnlocked = false;
    private bool screenActivated = false;
    private bool cupClueTriggered = false;

    private DoorController doorController;
    private GameObject screenObject;

    // ---------- 自动创建 ----------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateAndSubscribe()
    {
        if (Instance == null)
        {
            // 查找场景中是否已有 PuzzleManager
            PuzzleManager existing = FindObjectOfType<PuzzleManager>();
            if (existing != null)
            {
                Instance = existing;
                Instance.SubscribeEvents();
                Debug.Log("PuzzleManager found in scene and subscribed.");
                return;
            }

            // 否则创建新对象
            GameObject go = new GameObject("PuzzleManager");
            Instance = go.AddComponent<PuzzleManager>();
            DontDestroyOnLoad(go);
            Instance.SubscribeEvents();
            Debug.Log("PuzzleManager auto-created and subscribed.");
        }
        else
        {
            Instance.SubscribeEvents();
        }
    }

    private void Awake()
    {
        // 如果 Instance 为空，设为自己（防止重复）
        if (Instance == null)
        {
            Instance = this;
            SubscribeEvents();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void SubscribeEvents()
    {
        if (PuzzleEventManager.Instance != null)
        {
            PuzzleEventManager.Instance.OnItemGrabbed -= OnItemGrabbedHandler; // 防止重复订阅
            PuzzleEventManager.Instance.OnItemDropped -= OnItemDroppedHandler;
            PuzzleEventManager.Instance.OnItemGrabbed += OnItemGrabbedHandler;
            PuzzleEventManager.Instance.OnItemDropped += OnItemDroppedHandler;
            Debug.Log("PuzzleManager subscribed to events.");
        }
        else
        {
            Debug.LogWarning("PuzzleEventManager.Instance is null, will retry later.");
            // 可选：延迟重试
            Invoke(nameof(SubscribeEvents), 0.5f);
        }
    }

    private void Start()
    {
        doorController = FindDoor();
        screenObject = FindScreen();
    }

    private void OnDestroy()
    {
        if (PuzzleEventManager.Instance != null)
        {
            PuzzleEventManager.Instance.OnItemGrabbed -= OnItemGrabbedHandler;
            PuzzleEventManager.Instance.OnItemDropped -= OnItemDroppedHandler;
        }
        if (Instance == this) Instance = null;
    }

    // ---------- 事件处理 ----------
    private void OnItemGrabbedHandler(string objectId)
    {
        Debug.Log($"拾取事件: {objectId}");
        collectedItems.Add(objectId);
        CheckDoorCollection();
    }

    private void OnItemDroppedHandler(string objectId, GameObject surface)
    {
        if (surface == null) return;
        string surfaceId = GetRootObjectId(surface);
        Debug.Log($"放置事件: {objectId} 放在了 {(surfaceId ?? surface.name)} 上");

        // 茶杯放任意座椅
        if (!cupClueTriggered && objectId == "cup_01" && surfaceId != null && surfaceId.StartsWith(seatIdPrefix))
        {
            TriggerCupOnSeatClue();
        }

        // 遥控器放屏幕
        if (objectId == remoteId && surfaceId == screenId)
        {
            ActivateScreen();
        }
    }

    // ---------- 谜题逻辑 ----------
    private void CheckDoorCollection()
    {
        if (doorUnlocked) return;

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
            Renderer r = screenObject.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = Color.green;
                Debug.Log("屏幕已激活（绿色）");
            }
        }
        else
        {
            Debug.LogWarning("屏幕对象未找到，无法激活。");
        }
        screenActivated = true;
    }

    private void TriggerCupOnSeatClue()
    {
        cupClueTriggered = true;

        // 显示提示
        GameResultUI ui = GameResultUI.GetOrCreate();
        if (ui != null)
            ui.ShowMessage("你拿起茶杯，发现下面压着一张纸条：密码第一位是 3！");

        // 加入背包
        InventoryManager inv = InventoryManager.GetOrCreate();
        if (inv != null)
        {
            InventoryItem clue = new InventoryItem("cup_clue_note", "茶杯下的纸条", "note", "密码第一位是 3。");
            inv.AddItem(clue);
        }

        Debug.Log("茶杯座椅线索已触发！");
    }

    // ---------- 辅助 ----------
    private string GetRootObjectId(GameObject obj)
    {
        if (obj == null) return null;

        ObjectIdentity id = obj.GetComponent<ObjectIdentity>();
        if (id != null && !string.IsNullOrWhiteSpace(id.ObjectId))
            return id.ObjectId;

        Transform current = obj.transform.parent;
        while (current != null)
        {
            id = current.GetComponent<ObjectIdentity>();
            if (id != null && !string.IsNullOrWhiteSpace(id.ObjectId))
                return id.ObjectId;
            current = current.parent;
        }
        return null;
    }

    private DoorController FindDoor()
    {
        ObjectIdentity[] identities = FindObjectsOfType<ObjectIdentity>();
        foreach (var identity in identities)
        {
            if (identity.ObjectId == doorId)
            {
                DoorController door = identity.GetComponent<DoorController>();
                if (door != null) return door;
            }
        }
        return FindObjectOfType<DoorController>();
    }

    private GameObject FindScreen()
    {
        ObjectIdentity[] identities = FindObjectsOfType<ObjectIdentity>();
        foreach (var identity in identities)
        {
            if (identity.ObjectId == screenId)
                return identity.gameObject;
        }
        return GameObject.Find(screenId);
    }
}