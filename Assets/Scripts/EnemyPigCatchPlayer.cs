using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPigCatchPlayer : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private bool hasCaughtPlayer = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasCaughtPlayer)
        {
            return;
        }

        if (other.CompareTag(playerTag))
        {
            hasCaughtPlayer = true;

            Debug.Log("EnemyPig caught the player. Restarting scene.");

            Time.timeScale = 1f;

            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}