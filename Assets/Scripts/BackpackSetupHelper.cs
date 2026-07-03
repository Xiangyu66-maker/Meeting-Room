using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/Backpack Setup Helper")]
public sealed class BackpackSetupHelper : MonoBehaviour
{
    [SerializeField] private bool setupOnAwake = true;

    private const string CabinetObjectId = "cabinet_01";

    private void Awake()
    {
        if (setupOnAwake)
        {
            SetupBackpackSystem();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SetupAfterSceneLoad()
    {
        SetupBackpackSystem();
    }

    [ContextMenu("Setup Backpack System")]
    public static void SetupBackpackSystem()
    {
        GameObject root = FindOrCreateSystemRoot();

        InventoryManager inventoryManager = root.GetComponent<InventoryManager>();
        if (inventoryManager == null)
        {
            inventoryManager = root.AddComponent<InventoryManager>();
        }

        BackpackUI backpackUI = root.GetComponent<BackpackUI>();
        if (backpackUI == null)
        {
            backpackUI = root.AddComponent<BackpackUI>();
        }

        backpackUI.Configure(inventoryManager);

        BackpackInputController inputController = root.GetComponent<BackpackInputController>();
        if (inputController == null)
        {
            inputController = root.AddComponent<BackpackInputController>();
        }

        inputController.Configure(backpackUI);
        EnsureCabinetPickup();
    }

    private static void EnsureCabinetPickup()
    {
        GameObject cabinet = FindCabinetObject();
        if (cabinet == null)
        {
            Debug.LogWarning("Backpack setup could not find cabinet_01 / Openable Cabinet for clue note pickup.");
            return;
        }

        if (cabinet.GetComponent<ClueNotePickup>() == null)
        {
            cabinet.AddComponent<ClueNotePickup>();
            Debug.Log("Added ClueNotePickup to cabinet_01.", cabinet);
        }

        if (cabinet.GetComponent<InteractableObject>() == null)
        {
            cabinet.AddComponent<InteractableObject>();
            Debug.Log("Added InteractableObject to cabinet_01 for backpack note pickup.", cabinet);
        }

        if (cabinet.GetComponent<Collider>() == null && cabinet.GetComponentInChildren<Collider>() == null)
        {
            cabinet.AddComponent<BoxCollider>();
            Debug.Log("Added BoxCollider to cabinet_01 for backpack note pickup.", cabinet);
        }
    }

    private static GameObject FindCabinetObject()
    {
        ObjectIdentity[] identities = FindObjectIdentities();
        foreach (ObjectIdentity identity in identities)
        {
            if (identity != null && identity.ObjectId == CabinetObjectId)
            {
                return identity.gameObject;
            }
        }

        return FindSceneObjectByName("Openable Cabinet");
    }

    private static ObjectIdentity[] FindObjectIdentities()
    {
#if UNITY_2023_1_OR_NEWER
        return FindObjectsByType<ObjectIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        return FindObjectsOfType<ObjectIdentity>(true);
#endif
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            return null;
        }

        GameObject[] roots = activeScene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            GameObject match = FindChildByName(root.transform, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static GameObject FindChildByName(Transform root, string objectName)
    {
        if (root.name == objectName)
        {
            return root.gameObject;
        }

        foreach (Transform child in root)
        {
            GameObject match = FindChildByName(child, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static GameObject FindOrCreateSystemRoot()
    {
        GameObject existing = FindSceneObjectByName("Backpack System");
        if (existing != null)
        {
            return existing;
        }

        return new GameObject("Backpack System");
    }
}
