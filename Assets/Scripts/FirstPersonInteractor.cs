using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/First Person Interactor")]
public sealed class FirstPersonInteractor : MonoBehaviour
{
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private float interactionRange = 2f;   // �û�Ҫ��С��2m����ΪĬ��2m
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode grabKey = KeyCode.F;   // ����ʰȡ/���ü�
    [SerializeField] private bool showDebugPrompt = true;

    private InteractableObject currentTarget;
    private InteractableObject lastLoggedTarget;
    private GrabbableObject currentGrabbableTarget;
    private GrabbableObject heldObject;                     // ��ǰ���е�����

    private void Awake()
    {
        ResolveCamera();
    }

    private void Update()
    {
        // �����������ģʽ��������н���
        if (KeypadController.HasActiveInput)
        {
            currentTarget = null;
            currentGrabbableTarget = null;
            return;
        }

        ResolveCamera();

        // ---- ԭ�н����߼���E���� ----
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

        // ---- ����ץȡ/�����߼���F���� ----
        // ����ʰȡĿ�꣨����û�г�������ʱ�ż�⣩
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
                // �ѳ������� �� ����
                heldObject.Drop();
                heldObject = null;
            }
            else if (currentGrabbableTarget != null)
            {
                // δ��������׼��ʰȡ���� �� ʰȡ
                currentGrabbableTarget.Grab(interactionCamera.transform);
                heldObject = currentGrabbableTarget;
            }
        }
    }

    /// <summary>
    /// ���ҿɽ�������ԭ�У�
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
    /// ���ҿ�ʰȡ����������
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

        // ��ʾ������ʾ��E����
        if (currentTarget != null)
        {
           GUI.Label(new Rect((Screen.width - 180f) * 0.5f, Screen.height - 72f, 180f, 28f), "Press E to interact | Q for VLM");
        }

        // ��ʾץȡ/������ʾ��F����
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