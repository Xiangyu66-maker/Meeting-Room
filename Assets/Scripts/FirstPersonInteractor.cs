using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/First Person Interactor")]
public sealed class FirstPersonInteractor : MonoBehaviour
{
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool showDebugPrompt = true;

    private InteractableObject currentTarget;
    private InteractableObject lastLoggedTarget;

    private void Awake()
    {
        ResolveCamera();
    }

    private void Update()
    {
        if (KeypadController.HasActiveInput || IsBackpackOpen())
        {
            currentTarget = null;
            return;
        }

        ResolveCamera();
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
    }

    private InteractableObject FindLookTarget()
    {
        if (interactionCamera == null)
        {
            return null;
        }

        Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<InteractableObject>();
    }

    private void ResolveCamera()
    {
        if (interactionCamera != null)
        {
            return;
        }

        interactionCamera = GetComponent<Camera>();
        if (interactionCamera != null)
        {
            return;
        }

        interactionCamera = GetComponentInChildren<Camera>();
        if (interactionCamera != null)
        {
            return;
        }

        interactionCamera = Camera.main;
        if (interactionCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            interactionCamera = cameras.Length > 0 ? cameras[0] : null;
        }
    }

    private void OnGUI()
    {
        if (!showDebugPrompt || currentTarget == null || KeypadController.HasActiveInput || IsBackpackOpen())
        {
            return;
        }

        // TODO: Replace OnGUI prompt with project UI once the guidance UI layer exists.
        GUI.Label(new Rect((Screen.width - 180f) * 0.5f, Screen.height - 72f, 180f, 28f), "Press E to interact");
    }

    private static bool IsBackpackOpen()
    {
        BackpackUI backpack = BackpackUI.Instance;
        return backpack != null && backpack.IsOpen;
    }
}
