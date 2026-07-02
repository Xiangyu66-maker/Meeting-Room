using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Conference Room/Game State Manager")]
public sealed class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Success,
        Failed
    }

    public float timeLimitSeconds = 600f;
    public bool doorUnlocked;
    public GameState currentState = GameState.Playing;

    [SerializeField] private GameResultUI resultUI;

    private float startTime;
    private float frozenElapsedTime;

    private void Awake()
    {
        startTime = Time.time;
        frozenElapsedTime = 0f;
        currentState = GameState.Playing;
        doorUnlocked = false;

        EnsureResultUI();

        if (resultUI != null)
        {
            resultUI.Hide();
        }

        Debug.Log($"GameStateManager started. Time limit: {timeLimitSeconds:0} seconds.", this);
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        frozenElapsedTime = Time.time - startTime;
        if (frozenElapsedTime > timeLimitSeconds)
        {
            FailGame();
        }
    }

    public void NotifyDoorUnlocked()
    {
        if (doorUnlocked)
        {
            return;
        }

        doorUnlocked = true;
        Debug.Log("GameStateManager notified: door unlocked.", this);
    }

    public void TryTriggerSuccess()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        if (!doorUnlocked)
        {
            Debug.Log("Exit reached, but door is still locked.", this);
            return;
        }

        SucceedGame();
    }

    public void SucceedGame()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        frozenElapsedTime = Time.time - startTime;
        currentState = GameState.Success;

        EnsureResultUI();
        resultUI.ShowSuccess();

        Debug.Log("Game Success triggered.", this);
    }

    public void FailGame()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        frozenElapsedTime = Time.time - startTime;
        currentState = GameState.Failed;

        EnsureResultUI();
        resultUI.ShowFailure();

        Debug.Log("Game Failed: time limit exceeded.", this);
    }

    public float GetElapsedTime()
    {
        if (currentState == GameState.Playing)
        {
            return Time.time - startTime;
        }

        return frozenElapsedTime;
    }

    public bool IsGameOver()
    {
        return currentState != GameState.Playing;
    }

    private void EnsureResultUI()
    {
        if (resultUI != null)
        {
            return;
        }

        resultUI = GetComponent<GameResultUI>();
        if (resultUI == null)
        {
            resultUI = FindObjectOfType<GameResultUI>();
        }

        if (resultUI == null)
        {
            resultUI = gameObject.AddComponent<GameResultUI>();
            Debug.Log("GameResultUI was missing, so GameStateManager added one for player-view result prompts.", this);
        }
    }
}
