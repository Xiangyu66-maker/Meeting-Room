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

        // 先把玩家传送到储物柜里面
        if (hidePoint != null)
        {
            player.position = hidePoint.position;
            player.rotation = hidePoint.rotation;
        }
        else
        {
            Debug.LogWarning("LockerHideSpot: HidePoint is not assigned.");
        }

        // 关键部分：
        // 只冻结玩家的位置，不冻结旋转
        // 这样玩家不能走路，但可以转视角
        if (playerRigidbody != null)
        {
            originalConstraints = playerRigidbody.constraints;

            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;

            playerRigidbody.constraints =
                RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            Debug.LogWarning("LockerHideSpot: Player Rigidbody not found.");
        }

        PlayerHideState.SetHidden(true);

        ShowPrompt("Press E to exit");

        Debug.Log("Player entered the locker. Player can look around.");
    }

    private void ExitLocker()
    {
        playerIsInsideLocker = false;

        // 先把玩家传送到柜子外面
        if (exitPoint != null)
        {
            player.position = exitPoint.position;
            player.rotation = exitPoint.rotation;
        }
        else
        {
            Debug.LogWarning("LockerHideSpot: ExitPoint is not assigned.");
        }

        // 恢复原本 Rigidbody 设置
        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = originalConstraints;

            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        PlayerHideState.SetHidden(false);

        ShowPrompt("Press E to hide");

        Debug.Log("Player exited the locker.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            player = other.transform;
            playerInRange = true;

            if (!playerIsInsideLocker)
            {
                ShowPrompt("Press E to hide");
            }

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