using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
<<<<<<< Updated upstream
    public static PuzzleManager Instance { get; private set; }

=======
>>>>>>> Stashed changes
    [Header("谜题配置")]
    [SerializeField] private string[] requiredItemsForDoor = { "document_01", "remote_01" };
    [SerializeField] private string remoteId = "remote_01";
    [SerializeField] private string screenId = "screen_01";
    [SerializeField] private string doorId = "locked_door_01";
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    [Header("座椅配置")]
    [SerializeField] private string seatIdPrefix = "chair_";

    private HashSet<string> collectedItems = new HashSet<string>();
    private bool doorUnlocked = false;
    private bool screenActivated = false;
<<<<<<< Updated upstream
    private bool cupClueTriggered = false;
=======
    private bool cupClueTriggered = false;  // 新增这行
>>>>>>> Stashed changes

    private DoorController doorController;
    private GameObject screenObject;

    // ---------- 自动创建 ----------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateAndSubscribe()
    {
<<<<<<< Updated upstream
        if (Instance == null)
=======
        doorController = FindDoor();
        screenObject = FindScreen();

        if (PuzzleEventManager.Instance != null)
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    // ---------- 事件处理 ----------
    private void OnItemGrabbedHandler(string objectId)
    {
        Debug.Log($"拾取事件: {objectId}");
        collectedItems.Add(objectId);
=======
    private void OnItemGrabbedHandler(string objectId)
    {
        Debug.Log($"拾取事件: {objectId}");

        collectedItems.Add(objectId);

>>>>>>> Stashed changes
        CheckDoorCollection();
    }

    private void OnItemDroppedHandler(string objectId, GameObject surface)
    {
        if (surface == null) return;
<<<<<<< Updated upstream
        string surfaceId = GetRootObjectId(surface);
        Debug.Log($"放置事件: {objectId} 放在了 {(surfaceId ?? surface.name)} 上");

        // 茶杯放任意座椅
        if (!cupClueTriggered && objectId == "cup_01" && surfaceId != null && surfaceId.StartsWith(seatIdPrefix))
        {
            TriggerCupOnSeatClue();
        }

        // 遥控器放屏幕
=======

        // 获取表面物体的根级 ObjectId（向上查找）
        string surfaceId = GetRootObjectId(surface);
        Debug.Log($"放置事件: {objectId} 放在了 {(surfaceId ?? surface.name)} 上");

        // 原有逻辑：遥控器放屏幕
>>>>>>> Stashed changes
        if (objectId == remoteId && surfaceId == screenId)
        {
            ActivateScreen();
        }
<<<<<<< Updated upstream
    }

    // ---------- 谜题逻辑 ----------
    private void CheckDoorCollection()
    {
        if (doorUnlocked) return;
=======

        // 新增：茶杯放在任意座椅上触发线索
        if (!cupClueTriggered && objectId == "cup_01" && surfaceId != null && surfaceId.StartsWith(seatIdPrefix))
        {
            TriggerCupOnSeatClue();
        }
    }

    private void TriggerCupOnSeatClue()
    {
        cupClueTriggered = true;
        GameResultUI ui = GameResultUI.GetOrCreate();
        if (ui != null)
            ui.ShowMessage("你拿起茶杯，发现下面压着一张纸条：密码第一位是 3！");

        InventoryManager inv = InventoryManager.GetOrCreate();
        if (inv != null)
        {
            InventoryItem clue = new InventoryItem("cup_clue_note", "茶杯下的纸条", "note", "密码第一位是 3。");
            inv.AddItem(clue);
        }
        Debug.Log("茶杯座椅线索已触发！");
    }

    private void CheckDoorCollection()
    {
        if (doorUnlocked) return; 
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
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

=======

    }

>>>>>>> Stashed changes
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

        // 在 PuzzleManager 类中添加此方法
    private string GetRootObjectId(GameObject obj)
    {
        if (obj == null) return null;

        // 首先检查自身
        ObjectIdentity identity = obj.GetComponent<ObjectIdentity>();
        if (identity != null && !string.IsNullOrWhiteSpace(identity.ObjectId))
            return identity.ObjectId;

        // 向上遍历父物体
        Transform current = obj.transform.parent;
        while (current != null)
        {
            identity = current.GetComponent<ObjectIdentity>();
            if (identity != null && !string.IsNullOrWhiteSpace(identity.ObjectId))
                return identity.ObjectId;
            current = current.parent;
        }
        return null;
    }

    private void Awake()
    {
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
}