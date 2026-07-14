using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/First Person Interactor")]
public sealed class FirstPersonInteractor : MonoBehaviour
{
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private float interactionRange = 2f;   // 用户要求小于2m，设为默认2m
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode vlmKey = KeyCode.Q;
    [SerializeField] private KeyCode grabKey = KeyCode.F;   // 新增拾取/放置键
    [SerializeField] private bool showDebugPrompt = true;

    private InteractableObject currentTarget;
    private InteractableObject lastLoggedTarget;
    private GrabbableObject currentGrabbableTarget;
    private GrabbableObject heldObject;                     // 当前持有的物体

    private void Awake()
    {
        ResolveCamera();
    }

    private void Update()
    {
        // 如果键盘输入模式激活，不进行交互
        if (KeypadController.HasActiveInput)
        {
            currentTarget = null;
            currentGrabbableTarget = null;
            return;
        }

        ResolveCamera();

        // ---- 原有交互逻辑（E键） ----
        currentTarget = FindLookTarget();

        if (currentTarget != lastLoggedTarget)
        {
            lastLoggedTarget = currentTarget;
            if (currentTarget != null)
            {
                Debug.Log($"Looking at interactable object: {currentTarget.ObjectId}", currentTarget);
            }
        }

        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.Interact();
        }

        if (currentTarget != null && Input.GetKeyDown(vlmKey))
        {
            GptVisionInteractionManager manager = GptVisionInteractionManager.Instance;
            if (manager != null)
            {
                manager.AnalyzeObject(currentTarget.gameObject, currentTarget.ObjectId, currentTarget.Description);
            }
        }

        // ---- 新增抓取/放置逻辑（F键） ----
        // 检测可拾取目标（仅当没有持有物体时才检测）
        if (heldObject == null)
        {
            currentGrabbableTarget = FindGrabbableTarget();
        }
        else
        {
            currentGrabbableTarget = null;
        }

        if (Input.GetKeyDown(grabKey))
        {
            if (heldObject != null)
            {
                // 已持有物体 → 放置
                heldObject.Drop();
                heldObject = null;
            }
            else if (currentGrabbableTarget != null)
            {
                // 未持有且瞄准可拾取物体 → 拾取
                currentGrabbableTarget.Grab(interactionCamera.transform);
                heldObject = currentGrabbableTarget;
            }
        }
    }

    /// <summary>
    /// 查找可交互对象（原有）
    /// </summary>
    private InteractableObject FindLookTarget()
    {
        if (interactionCamera == null) return null;

        Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            return null;

        return hit.collider.GetComponentInParent<InteractableObject>();
    }

    /// <summary>
    /// 查找可拾取对象（新增）
    /// </summary>
    private GrabbableObject FindGrabbableTarget()
    {
        if (interactionCamera == null) return null;

        Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            return null;

        return hit.collider.GetComponentInParent<GrabbableObject>();
    }

    private void ResolveCamera()
    {
        if (interactionCamera != null) return;

        interactionCamera = GetComponent<Camera>();
        if (interactionCamera != null) return;

        interactionCamera = GetComponentInChildren<Camera>();
        if (interactionCamera != null) return;

        interactionCamera = Camera.main;
        if (interactionCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            interactionCamera = cameras.Length > 0 ? cameras[0] : null;
        }
    }

    private void OnGUI()
    {
        if (!showDebugPrompt) return;
        if (KeypadController.HasActiveInput) return;

        // 显示交互提示（E键）
        if (currentTarget != null)
        {
            GUI.Label(new Rect((Screen.width - 180f) * 0.5f, Screen.height - 72f, 180f, 28f), "Press E to interact | Q for VLM");
        }

        // 显示抓取/放置提示（F键）
        if (heldObject != null)
        {
            GUI.Label(new Rect((Screen.width - 200f) * 0.5f, Screen.height - 108f, 200f, 28f), "Press F to drop");
        }
        else if (currentGrabbableTarget != null)
        {
            GUI.Label(new Rect((Screen.width - 200f) * 0.5f, Screen.height - 108f, 200f, 28f), "Press F to pick up");
        }
    }
}