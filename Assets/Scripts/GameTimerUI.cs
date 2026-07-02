using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/Game Timer UI")]
public sealed class GameTimerUI : MonoBehaviour
{
    [SerializeField] private GameStateManager manager;
    [SerializeField] private bool useTopPlacement = true;
    [SerializeField] private float lowTimeThresholdSeconds = 60f;
    [SerializeField] private bool useTextMeshProWhenAvailable = true;

    private RectTransform fillRect;
    private Image fillImage;
    private Component tmpText;
    private Text uiText;

    private void Awake()
    {
        ResolveManager();
        EnsureUI();
        UpdateTimerVisuals();
    }

    private void Update()
    {
        ResolveManager();
        if (manager == null)
        {
            return;
        }

        UpdateTimerVisuals();

        if (manager.IsGameOver())
        {
            enabled = false;
            return;
        }

        if (GetRemainingSeconds() <= 0f)
        {
            manager.FailGame();
            enabled = false;
        }
    }

    private void ResolveManager()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<GameStateManager>();
        }
    }

    private void EnsureUI()
    {
        if (fillRect != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Game Timer Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject timerRoot = new GameObject("Timer Root", typeof(RectTransform));
        timerRoot.transform.SetParent(canvasObject.transform, false);

        RectTransform rootRect = timerRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = useTopPlacement ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rootRect.anchorMax = rootRect.anchorMin;
        rootRect.pivot = useTopPlacement ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = useTopPlacement ? new Vector2(0f, -18f) : new Vector2(0f, 18f);
        rootRect.sizeDelta = new Vector2(760f, 56f);

        GameObject background = new GameObject("Timer Bar Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(timerRoot.transform, false);

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0f);
        backgroundRect.pivot = new Vector2(0.5f, 0f);
        backgroundRect.anchoredPosition = new Vector2(0f, 4f);
        backgroundRect.sizeDelta = new Vector2(760f, 24f);

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.55f);

        GameObject fill = new GameObject("Timer Bar Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(background.transform, false);

        fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.22f, 0.82f, 0.42f, 0.95f);

        GameObject textObject = new GameObject("Timer Text", typeof(RectTransform));
        textObject.transform.SetParent(timerRoot.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -2f);
        textRect.sizeDelta = new Vector2(760f, 28f);

        if (!TryCreateTextMeshProText(textObject))
        {
            uiText = textObject.AddComponent<Text>();
            uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            uiText.fontSize = 22;
            uiText.color = Color.white;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    private void UpdateTimerVisuals()
    {
        if (fillRect == null)
        {
            return;
        }

        float remaining = GetRemainingSeconds();
        float limit = manager != null ? Mathf.Max(0.01f, manager.timeLimitSeconds) : 600f;
        float fillValue = Mathf.Clamp01(remaining / limit);

        fillRect.anchorMax = new Vector2(fillValue, 1f);

        bool lowTime = remaining <= lowTimeThresholdSeconds;
        Color timerColor = lowTime ? new Color(1f, 0.25f, 0.18f, 0.95f) : new Color(0.22f, 0.82f, 0.42f, 0.95f);
        if (fillImage != null)
        {
            fillImage.color = timerColor;
        }

        string label = $"Time Left: {FormatTime(remaining)}";
        if (tmpText != null)
        {
            TrySetProperty(tmpText, "text", label);
            TrySetProperty(tmpText, "color", lowTime ? new Color(1f, 0.85f, 0.72f, 1f) : Color.white);
        }
        else if (uiText != null)
        {
            uiText.text = label;
            uiText.color = lowTime ? new Color(1f, 0.85f, 0.72f, 1f) : Color.white;
        }
    }

    private float GetRemainingSeconds()
    {
        if (manager == null)
        {
            return 600f;
        }

        float limit = Mathf.Max(0f, manager.timeLimitSeconds);
        return Mathf.Clamp(limit - manager.GetElapsedTime(), 0f, limit);
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }

    private bool TryCreateTextMeshProText(GameObject textObject)
    {
        if (!useTextMeshProWhenAvailable)
        {
            return false;
        }

        Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        if (tmpType == null)
        {
            return false;
        }

        tmpText = textObject.AddComponent(tmpType);
        TrySetProperty(tmpText, "fontSize", 22f);
        TrySetProperty(tmpText, "color", Color.white);
        TrySetProperty(tmpText, "enableWordWrapping", false);

        Type alignmentType = Type.GetType("TMPro.TextAlignmentOptions, Unity.TextMeshPro");
        if (alignmentType != null)
        {
            TrySetProperty(tmpText, "alignment", Enum.Parse(alignmentType, "Center"));
        }

        return true;
    }

    private static void TrySetProperty(Component component, string propertyName, object value)
    {
        if (component == null)
        {
            return;
        }

        var property = component.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(component, value, null);
        }
    }
}
