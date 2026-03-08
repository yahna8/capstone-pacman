using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text pelletsText;

    private void OnEnable()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
            scoreManager.OnPelletsRemainingChanged += HandlePelletsChanged;

            HandleScoreChanged(scoreManager.Score);
            HandlePelletsChanged(scoreManager.PelletsRemaining);
        }
    }

    private void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= HandleScoreChanged;
            scoreManager.OnPelletsRemainingChanged -= HandlePelletsChanged;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {newScore}";
    }

    private void HandlePelletsChanged(int remaining)
    {
        if (pelletsText != null)
            pelletsText.text = $"Pellets Remaining: {remaining}";
    }
}
