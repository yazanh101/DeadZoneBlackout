
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TMP_Text gameOverStatsText;

    void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        // Display final score and saved high score
        if (gameOverStatsText != null &&
            PointsManager.Instance != null)
        {
            gameOverStatsText.text =
                "SCORE: " + PointsManager.Instance.TotalEarnedPoints +
                "\nBEST: " + PointsManager.Instance.HighScore;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Freeze the game
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
