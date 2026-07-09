using TMPro;
using UnityEngine;

public class LockerHideSpot : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Hide Position")]
    [SerializeField] private Transform hidePoint;
    [SerializeField] private Transform exitPoint;

    [Header("UI")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;

    private Transform player;
    private Rigidbody playerRigidbody;
    private Behaviour playerMovementScript;

    private bool playerInRange = false;
    private bool playerIsInsideLocker = false;

    private RigidbodyConstraints originalConstraints;

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        if (!playerInRange || player == null)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("E key pressed near locker.");
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        if (!playerIsInsideLocker)
        {
            EnterLocker();
        }
        else
        {
            ExitLocker();
        }
    }

    private void EnterLocker()
    {
        playerIsInsideLocker = true;

        playerRigidbody = player.GetComponent<Rigidbody>();

        if (playerRigidbody != null)
        {
            originalConstraints = playerRigidbody.constraints;

            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        FindAndDisableMovementScript();

        if (hidePoint != null)
        {
            player.position = hidePoint.position;
            player.rotation = hidePoint.rotation;
        }

        PlayerHideState.SetHidden(true);

        ShowPrompt("Press E to exit");

        Debug.Log("Player entered the locker.");
    }

    private void ExitLocker()
    {
        playerIsInsideLocker = false;

        if (exitPoint != null)
        {
            player.position = exitPoint.position;
            player.rotation = exitPoint.rotation;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = originalConstraints;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        PlayerHideState.SetHidden(false);

        ShowPrompt("Press E to hide");

        Debug.Log("Player exited the locker.");
    }

    private void FindAndDisableMovementScript()
    {
        Behaviour[] scripts = player.GetComponents<Behaviour>();

        foreach (Behaviour script in scripts)
        {
            if (script == null)
            {
                continue;
            }

            string scriptName = script.GetType().Name.ToLower();

            if (scriptName.Contains("firstperson") ||
                scriptName.Contains("controller") ||
                scriptName.Contains("movement"))
            {
                playerMovementScript = script;
                playerMovementScript.enabled = false;

                Debug.Log("Disabled player movement script: " + script.GetType().Name);
                return;
            }
        }

        Debug.LogWarning("No player movement script found to disable.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            player = other.transform;
            playerInRange = true;

            ShowPrompt("Press E to hide");

            Debug.Log("Player entered locker range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!playerIsInsideLocker)
            {
                playerInRange = false;
                player = null;

                HidePrompt();

                Debug.Log("Player left locker range.");
            }
        }
    }

    private void ShowPrompt(string message)
    {
        if (promptObject != null)
        {
            promptObject.SetActive(true);
        }

        if (promptText != null)
        {
            promptText.text = message;
        }
    }

    private void HidePrompt()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }
}