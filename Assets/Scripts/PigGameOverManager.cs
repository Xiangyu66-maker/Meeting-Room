using UnityEngine;
using UnityEngine.SceneManagement;

public class PigGameOverManager : MonoBehaviour
{
    public static PigGameOverManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver = false;

    private void Awake()
    {
        Instance = this;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // 游戏开始时正常锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowGameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PigGameOverManager: GameOverPanel is not assigned.");
        }

        // 暂停游戏
        Time.timeScale = 0f;

        // 显示鼠标，让玩家可以点击 Restart 按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Debug.Log("Restart button clicked.");

        Time.timeScale = 1f;

        // 重启前恢复鼠标状态
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}