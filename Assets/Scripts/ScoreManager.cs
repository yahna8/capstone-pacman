using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private int pelletPoints = 10;
    [SerializeField] private int powerPelletPoints = 50;
    [SerializeField] private int ghostEatenPoints = 200;

    private int score;
    private int pelletsRemaining;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnPelletsRemainingChanged;
    public event Action<bool> OnPelletConsumed;
    public event Action OnPowerPelletConsumed;
    public event Action<int> OnGhostEaten;

    public int Score => score;
    public int PelletsRemaining => pelletsRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RecountPelletsInScene();

        if (gameStateManager != null)
        {
            gameStateManager.SetRemainingPellets(pelletsRemaining);
        }
        
        ResetScore();
    }

    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);
    }

    public void AddPoints(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void RecountPelletsInScene()
    {
        int normal = GameObject.FindGameObjectsWithTag("Pellet").Length;
        int power = GameObject.FindGameObjectsWithTag("PowerPellet").Length;
        pelletsRemaining = normal + power;
        OnPelletsRemainingChanged?.Invoke(pelletsRemaining);
    }

    public void NotifyPelletConsumed(bool isPowerPellet)
    {
        AddPoints(isPowerPellet ? powerPelletPoints : pelletPoints);

        pelletsRemaining = Mathf.Max(0, pelletsRemaining - 1);

        if (gameStateManager != null)
        {
            gameStateManager.SetRemainingPellets(pelletsRemaining);
        }

        OnPelletsRemainingChanged?.Invoke(pelletsRemaining);
        OnPelletConsumed?.Invoke(isPowerPellet);

        if (isPowerPellet)
            OnPowerPelletConsumed?.Invoke();
    }

    public void NotifyGhostEaten()
    {
        AddPoints(ghostEatenPoints);
        OnGhostEaten?.Invoke(ghostEatenPoints);
    }
}
