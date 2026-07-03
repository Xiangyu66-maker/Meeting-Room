using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/Backpack UI")]
public sealed class BackpackUI : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;

    private Canvas canvas;
    private RectTransform iconRoot;
    private GameObject redDot;
    private GameObject panelRoot;
    private RectTransform itemListRoot;
    private TextMeshProUGUI noteDisplayText;
    private TextMeshProUGUI emptyText;
    private string selectedItemId;
    private bool suppressInventoryRefresh;

    public static BackpackUI Instance { get; private set; }
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate BackpackUI found. Disabling duplicate.", this);
            enabled = false;
            return;
        }

        Instance = this;
        ResolveInventory();
        EnsureUI();
        Hide();
        UpdateNotificationDot();
    }

    private void OnEnable()
    {
        ResolveInventory();
        if (inventoryManager != null)
        {
            inventoryManager.InventoryChanged += HandleInventoryChanged;
        }
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.InventoryChanged -= HandleInventoryChanged;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static BackpackUI GetOrCreate()
    {
        if (Instance != null)
        {
            return Instance;
        }

        BackpackUI existing = FindBackpackUI();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject uiObject = new GameObject("Backpack UI");
        return uiObject.AddComponent<BackpackUI>();
    }

    public void Configure(InventoryManager manager)
    {
        if (inventoryManager != manager)
        {
            if (inventoryManager != null)
            {
                inventoryManager.InventoryChanged -= HandleInventoryChanged;
            }

            inventoryManager = manager;

            if (inventoryManager != null)
            {
                inventoryManager.InventoryChanged += HandleInventoryChanged;
            }
        }

        EnsureUI();
        RefreshPanel();
        UpdateNotificationDot();
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            CloseBackpack();
        }
        else
        {
            OpenBackpack();
        }
    }

    public void OpenBackpack()
    {
        EnsureUI();
        ResolveInventory();

        panelRoot.SetActive(true);

        suppressInventoryRefresh = true;
        MarkVisibleNotesRead();
        suppressInventoryRefresh = false;

        RefreshPanel();
        UpdateNotificationDot();
    }

    public void CloseBackpack()
    {
        EnsureUI();
        panelRoot.SetActive(false);
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void HandleInventoryChanged()
    {
        if (suppressInventoryRefresh)
        {
            return;
        }

        RefreshPanel();
        UpdateNotificationDot();
    }

    private void ResolveInventory()
    {
        if (inventoryManager != null)
        {
            return;
        }

        inventoryManager = InventoryManager.GetOrCreate();
    }

    private void EnsureUI()
    {
        if (canvas != null && iconRoot != null && panelRoot != null)
        {
            return;
        }

        canvas = FindOrCreateCanvas();
        EnsureEventSystem();
        CreateBackpackIcon();
        CreateBackpackPanel();
    }

    private Canvas FindOrCreateCanvas()
    {
        Canvas namedCanvas = FindCanvasByName("Backpack Canvas");
        if (namedCanvas != null)
        {
            namedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            namedCanvas.sortingOrder = 5100;
            if (namedCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                namedCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            return namedCanvas;
        }

        GameObject canvasObject = new GameObject("Backpack Canvas");
        Canvas createdCanvas = canvasObject.AddComponent<Canvas>();
        createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        createdCanvas.sortingOrder = 5100;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        return createdCanvas;
    }

    private static Canvas FindCanvasByName(string canvasName)
    {
#if UNITY_2023_1_OR_NEWER
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
#endif
        foreach (Canvas candidate in canvases)
        {
            if (candidate != null && candidate.name == canvasName)
            {
                return candidate;
            }
        }

        return null;
    }

    private static void EnsureEventSystem()
    {
#if UNITY_2023_1_OR_NEWER
        EventSystem existing = FindFirstObjectByType<EventSystem>();
#else
        EventSystem existing = FindObjectOfType<EventSystem>();
#endif
        if (existing != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void CreateBackpackIcon()
    {
        if (iconRoot != null)
        {
            return;
        }

        GameObject iconObject = CreateUIObject("Backpack Icon", canvas.transform);
        iconRoot = iconObject.GetComponent<RectTransform>();
        iconRoot.anchorMin = new Vector2(1f, 1f);
        iconRoot.anchorMax = new Vector2(1f, 1f);
        iconRoot.pivot = new Vector2(1f, 1f);
        iconRoot.anchoredPosition = new Vector2(-24f, -88f);
        iconRoot.sizeDelta = new Vector2(74f, 52f);

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.color = new Color(0.08f, 0.1f, 0.13f, 0.88f);

        Button iconButton = iconObject.AddComponent<Button>();
        iconButton.targetGraphic = iconImage;
        iconButton.onClick.AddListener(Toggle);

        TextMeshProUGUI iconText = CreateText("Bag", iconRoot, "Backpack Icon Text", 24f, Color.white, TextAlignmentOptions.Center);
        RectTransform iconTextRect = iconText.rectTransform;
        iconTextRect.anchorMin = Vector2.zero;
        iconTextRect.anchorMax = Vector2.one;
        iconTextRect.offsetMin = Vector2.zero;
        iconTextRect.offsetMax = Vector2.zero;

        GameObject dotObject = CreateUIObject("Backpack Red Dot", iconRoot);
        RectTransform dotRect = dotObject.GetComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(1f, 1f);
        dotRect.anchorMax = new Vector2(1f, 1f);
        dotRect.pivot = new Vector2(0.5f, 0.5f);
        dotRect.anchoredPosition = new Vector2(-5f, -5f);
        dotRect.sizeDelta = new Vector2(16f, 16f);

        Image dotImage = dotObject.AddComponent<Image>();
        dotImage.color = Color.red;
        dotImage.sprite = CreateCircleSprite(32, Color.white);
        redDot = dotObject;
    }

    private void CreateBackpackPanel()
    {
        if (panelRoot != null)
        {
            return;
        }

        panelRoot = CreateUIObject("Backpack Panel", canvas.transform);
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-24f, 0f);
        panelRect.sizeDelta = new Vector2(460f, 520f);

        Image panelImage = panelRoot.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.07f, 0.08f, 0.94f);

        TextMeshProUGUI title = CreateText("Backpack", panelRect, "Backpack Title", 30f, Color.white, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -18f);
        titleRect.sizeDelta = new Vector2(-36f, 44f);

        GameObject itemListObject = CreateUIObject("Backpack Item List", panelRect);
        itemListRoot = itemListObject.GetComponent<RectTransform>();
        itemListRoot.anchorMin = new Vector2(0f, 0f);
        itemListRoot.anchorMax = new Vector2(0f, 1f);
        itemListRoot.pivot = new Vector2(0f, 1f);
        itemListRoot.anchoredPosition = new Vector2(24f, -80f);
        itemListRoot.sizeDelta = new Vector2(160f, -128f);

        GameObject noteDisplayObject = CreateUIObject("Backpack Note Display", panelRect);
        RectTransform noteRect = noteDisplayObject.GetComponent<RectTransform>();
        noteRect.anchorMin = new Vector2(0f, 0f);
        noteRect.anchorMax = new Vector2(1f, 1f);
        noteRect.offsetMin = new Vector2(204f, 78f);
        noteRect.offsetMax = new Vector2(-24f, -82f);

        Image noteImage = noteDisplayObject.AddComponent<Image>();
        noteImage.color = new Color(0.92f, 0.88f, 0.72f, 1f);

        noteDisplayText = CreateText(string.Empty, noteRect, "Backpack Note Text", 22f, Color.black, TextAlignmentOptions.TopLeft);
        RectTransform noteTextRect = noteDisplayText.rectTransform;
        noteTextRect.anchorMin = Vector2.zero;
        noteTextRect.anchorMax = Vector2.one;
        noteTextRect.offsetMin = new Vector2(18f, 18f);
        noteTextRect.offsetMax = new Vector2(-18f, -18f);
        noteDisplayText.textWrappingMode = TextWrappingModes.Normal;

        emptyText = CreateText("No notes collected.", panelRect, "Backpack Empty Text", 22f, Color.white, TextAlignmentOptions.Center);
        RectTransform emptyRect = emptyText.rectTransform;
        emptyRect.anchorMin = new Vector2(0f, 0.5f);
        emptyRect.anchorMax = new Vector2(1f, 0.5f);
        emptyRect.pivot = new Vector2(0.5f, 0.5f);
        emptyRect.anchoredPosition = new Vector2(0f, 0f);
        emptyRect.sizeDelta = new Vector2(-48f, 48f);

        TextMeshProUGUI closeText = CreateText("Press Tab to close", panelRect, "Backpack Close Text", 18f, Color.white, TextAlignmentOptions.Center);
        RectTransform closeRect = closeText.rectTransform;
        closeRect.anchorMin = new Vector2(0f, 0f);
        closeRect.anchorMax = new Vector2(1f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 18f);
        closeRect.sizeDelta = new Vector2(-36f, 34f);
    }

    private void RefreshPanel()
    {
        if (panelRoot == null || itemListRoot == null || noteDisplayText == null || inventoryManager == null)
        {
            return;
        }

        foreach (Transform child in itemListRoot)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItem> items = inventoryManager.GetItems();
        bool hasItems = items.Count > 0;
        emptyText.gameObject.SetActive(!hasItems);
        noteDisplayText.transform.parent.gameObject.SetActive(hasItems);

        if (!hasItems)
        {
            selectedItemId = null;
            noteDisplayText.text = string.Empty;
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedItemId) || !inventoryManager.HasItem(selectedItemId))
        {
            selectedItemId = items[0].itemId;
        }

        float y = 0f;
        foreach (InventoryItem item in items)
        {
            CreateItemButton(item, y);
            y -= 58f;
        }

        ShowItem(selectedItemId, false);
    }

    private void CreateItemButton(InventoryItem item, float y)
    {
        GameObject buttonObject = CreateUIObject(item.itemName, itemListRoot);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, y);
        buttonRect.sizeDelta = new Vector2(0f, 48f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = item.itemId == selectedItemId
            ? new Color(0.98f, 0.78f, 0.22f, 1f)
            : new Color(0.92f, 0.88f, 0.72f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        string capturedItemId = item.itemId;
        button.onClick.AddListener(() => ShowItem(capturedItemId, true));

        TextMeshProUGUI label = CreateText(item.itemName, buttonRect, "Item Label", 18f, Color.black, TextAlignmentOptions.Center);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);
    }

    private void ShowItem(string itemId, bool markRead)
    {
        if (inventoryManager == null)
        {
            return;
        }

        InventoryItem item = null;
        foreach (InventoryItem candidate in inventoryManager.GetItems())
        {
            if (candidate != null && candidate.itemId == itemId)
            {
                item = candidate;
                break;
            }
        }

        if (item == null)
        {
            return;
        }

        selectedItemId = item.itemId;
        noteDisplayText.text = $"{item.itemName}\n\n{item.content}";

        if (markRead)
        {
            inventoryManager.MarkItemRead(item.itemId);
            UpdateNotificationDot();
        }
    }

    private void MarkVisibleNotesRead()
    {
        if (inventoryManager == null)
        {
            return;
        }

        foreach (InventoryItem item in inventoryManager.GetItems())
        {
            if (item != null && item.itemType == "note")
            {
                inventoryManager.MarkItemRead(item.itemId);
            }
        }
    }

    private void UpdateNotificationDot()
    {
        if (redDot == null || inventoryManager == null)
        {
            return;
        }

        redDot.SetActive(inventoryManager.HasUnreadItems());
    }

    private TextMeshProUGUI CreateText(string value, Transform parent, string objectName, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius - 0.5f, radius - 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color pixelColor = distance <= radius ? color : Color.clear;
                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
    }

    private static BackpackUI FindBackpackUI()
    {
#if UNITY_2023_1_OR_NEWER
        return FindFirstObjectByType<BackpackUI>();
#else
        return FindObjectOfType<BackpackUI>();
#endif
    }
}
