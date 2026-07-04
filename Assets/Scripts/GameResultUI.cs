using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/Game Result UI")]
public sealed class GameResultUI : MonoBehaviour
{
    private const string SuccessMessage = "Game Success! You escaped the meeting room.";
    private const string FailureMessage = "Game Failed! Time limit exceeded.";

    [SerializeField] private bool useTextMeshProWhenAvailable = true;

    private GameObject resultRoot;
    private Component tmpText;
    private Text uiText;
    private string currentMessage;
    private bool showImmediateModeFallback;

    private void Awake()
    {
        EnsureUI();
        Hide();
    }

    public void ShowSuccess()
    {
        ShowMessage(SuccessMessage);
    }

    public void ShowFailure()
    {
        ShowMessage(FailureMessage);
    }

    public void ShowMessage(string message)
    {
        EnsureUI();
        SetText(message);
        currentMessage = message;
        showImmediateModeFallback = true;
        resultRoot.SetActive(true);
        Debug.Log($"Game result prompt shown: {message}", this);
    }

    public void Hide()
    {
        EnsureUI();
        resultRoot.SetActive(false);
        currentMessage = string.Empty;
        showImmediateModeFallback = false;
    }

    private void OnGUI()
    {
        if (!showImmediateModeFallback || string.IsNullOrEmpty(currentMessage))
        {
            return;
        }

        int previousDepth = GUI.depth;
        GUI.depth = -1000;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(24, Mathf.RoundToInt(Screen.height * 0.045f)),
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        boxStyle.normal.textColor = Color.white;

        float width = Mathf.Min(Screen.width * 0.82f, 980f);
        float height = Mathf.Min(Screen.height * 0.24f, 190f);
        Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUI.Box(rect, currentMessage, boxStyle);

        GUI.depth = previousDepth;
    }

    private void EnsureUI()
    {
        if (resultRoot != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Game Result Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        resultRoot = new GameObject("Game Result Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        resultRoot.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = resultRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = resultRoot.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.68f);

        GameObject textObject = new GameObject("Game Result Text", typeof(RectTransform));
        textObject.transform.SetParent(resultRoot.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(1200f, 220f);

        if (!TryCreateTextMeshProText(textObject))
        {
            uiText = textObject.AddComponent<Text>();
            uiText.font = GetBuiltinUIFont();
            uiText.fontSize = 56;
            uiText.fontStyle = FontStyle.Bold;
            uiText.color = Color.white;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(3f, -3f);
        }
    }

    private static Font GetBuiltinUIFont()
    {
        try
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (ArgumentException)
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
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
        TrySetProperty(tmpText, "fontSize", 42f);
        TrySetProperty(tmpText, "color", Color.white);
        TrySetProperty(tmpText, "enableWordWrapping", true);

        Type alignmentType = Type.GetType("TMPro.TextAlignmentOptions, Unity.TextMeshPro");
        if (alignmentType != null)
        {
            TrySetProperty(tmpText, "alignment", Enum.Parse(alignmentType, "Center"));
        }

        return true;
    }

    private void SetText(string message)
    {
        if (tmpText != null)
        {
            TrySetProperty(tmpText, "text", message);
            return;
        }

        if (uiText != null)
        {
            uiText.text = message;
        }
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
