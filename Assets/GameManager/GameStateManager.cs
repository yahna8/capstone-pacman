using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    GameBaseState currentState;

    // States
    public GameBootState BootState = new GameBootState();
    public GameCountdownState CountdownState = new GameCountdownState();
    public GameLevelCompleteState LevelCompleteState = new GameLevelCompleteState();
    public GameLifeLostState LifeLostState = new GameLifeLostState();
    public GameMenuState MenuState = new GameMenuState();
    public GameOverState OverState = new GameOverState();
    public GamePausedState PausedState = new GamePausedState();
    public GamePlayingState PlayingState = new GamePlayingState();

    // --- Simple game flags for now ---
    [Header("Game State Debug Flags (temporary)")]
    public bool StartPressed;          // menu -> countdown
    public bool ResumePressed;         // paused -> playing
    public bool QuitToMenuPressed;     // paused/gameover -> menu
    public bool RestartPressed;        // gameover -> countdown
    public bool PlayerIsDead;          // playing -> life lost
    public int RemainingPellets = 30;  // playing -> level complete

    [Header("Game Data (temporary)")]
    public int Lives = 3;

    [Header("Timers")]
    public float CountdownSeconds = 3f;
    public float LifeLostSeconds = 2f;
    public float LevelCompleteSeconds = 2f;

    [Header("Reset References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private ScoreManager scoreManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        currentState = BootState;
        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        // For testing state transitions without UI
        DebugHotkeys();

        currentState.UpdateState(this);
    }

    public void SwitchState(GameBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    public bool IsInState(GameBaseState state)
    {
        return currentState == state;
    }

    public bool IsPlaying()
    {
        return currentState == PlayingState;
    }

    public int GetLives()
    {
        return Lives;
    }

    public int GetRemainingPellets()
    {
        return RemainingPellets;
    }

    public void SetRemainingPellets(int pellets)
    {
        RemainingPellets = pellets;
    }

    public void NotifyPlayerDied()
    {
        PlayerIsDead = true;
    }

    public void OnCollisionEnter(Collision collision)
    {
        currentState.OnCollisionEnter(this);
    }

    public void ResetRun()
    {
        Lives = 3;
        PlayerIsDead = false;
        StartPressed = false;
        ResumePressed = false;
        QuitToMenuPressed = false;
        RestartPressed = false;

        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            scoreManager.RecountPelletsInScene();
            SetRemainingPellets(scoreManager.PelletsRemaining);
        }

        ResetPlayerPosition();
    }

    public void ResetPlayerPosition()
    {
        if (playerTransform == null || playerStartPoint == null) return;

        var xrOrigin = playerTransform.GetComponent<Unity.XR.CoreUtils.XROrigin>();

        if (xrOrigin != null)
        {
            // Disable CharacterController if used
            var cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Reset XR Origin position
            xrOrigin.transform.position = playerStartPoint.position;
            xrOrigin.transform.rotation = playerStartPoint.rotation;

            // Reset camera offset so head tracking doesn't offset you
            xrOrigin.CameraFloorOffsetObject.transform.localPosition = Vector3.zero;

            if (cc != null) cc.enabled = true;
        }
    }

    // --- Helpers to call from UI buttons later ---
    public void PressStart() => StartPressed = true;
    public void PressResume() => ResumePressed = true;
    public void PressQuitToMenu() => QuitToMenuPressed = true;
    public void PressRestart() => RestartPressed = true;

    private void DebugHotkeys()
    {
        // Only for testing on desktop, remove later:
        if (Input.GetKeyDown(KeyCode.Return)) StartPressed = true;     // Enter = Start
        if (Input.GetKeyDown(KeyCode.Escape))                          // Esc toggles pause/resume
        {
            if (currentState == PlayingState) SwitchState(PausedState);
            else if (currentState == PausedState) ResumePressed = true;
        }
        if (Input.GetKeyDown(KeyCode.K)) PlayerIsDead = true;          // K = simulate death
        if (Input.GetKeyDown(KeyCode.L)) RemainingPellets = 0;         // L = simulate win
        if (Input.GetKeyDown(KeyCode.R)) RestartPressed = true;        // R = restart (GameOver)
        if (Input.GetKeyDown(KeyCode.M)) QuitToMenuPressed = true;     // M = menu
    }
}
