
using UnityEngine;
using TMPro;

public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance;

    [Header("UI")]
    public TMP_Text pointsText;
    public TMP_Text highScoreText;

    [Header("Points")]
    public int points = 0;

    private int totalEarnedPoints = 0;
    private int highScore = 0;

    public int TotalEarnedPoints => totalEarnedPoints;
    public int HighScore => highScore;


    private const string HighScoreKey = "DeadZoneBlackout_HighScore";

    void Awake()
    {
        Instance = this;

        // Load saved high score
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0)
            return;

        // Points available for shopping
        points += amount;

        // Total points earned in this run
        totalEarnedPoints += amount;

        // Save a new record
        if (totalEarnedPoints > highScore)
        {
            highScore = totalEarnedPoints;

            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    public bool SpendPoints(int amount)
    {
        if (amount < 0 || points < amount)
            return false;

        points -= amount;

        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = "POINTS: " + points;

        if (highScoreText != null)
            highScoreText.text = "BEST: " + highScore;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
