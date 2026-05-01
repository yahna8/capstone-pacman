using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameStateManager gameStateManager;

    [Header("HUD")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private Text livesText;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource gameStartAudio;
    [SerializeField] private AudioSource gameMenuAudio;

    private bool settingsOpen = false;
    private bool wasInMenu = false;

    private void Start()
    {
        UpdateLivesUI();
        RefreshUI();
    }

    private void Update()
    {
        if (gameStateManager != null)
        {
            UpdateLivesUI();
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (gameStateManager == null) return;

        HideAllPanels();

        // Settings is handled separately from game state
        if (settingsOpen)
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
            return;
        }

        // Handle audio
        bool isInMenu = gameStateManager.IsInState(gameStateManager.MenuState);

        if (isInMenu)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            if (!wasInMenu && gameMenuAudio != null)
            {
                gameMenuAudio.Play();
            }
        }

        if (gameStateManager.IsInState(gameStateManager.MenuState))
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }
        else if (gameStateManager.IsInState(gameStateManager.PausedState))
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else if (gameStateManager.IsInState(gameStateManager.OverState))
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
        else if (gameStateManager.IsInState(gameStateManager.LevelCompleteState))
        {
            if (winPanel != null) winPanel.SetActive(true);
        }
        else
        {
            if (hudRoot != null) hudRoot.SetActive(true);
        }

        if (!isInMenu && wasInMenu)
        {
            if (gameMenuAudio != null)
                gameMenuAudio.Stop();
        }
        // Tracks state change
        wasInMenu = isInMenu;
    }

    private void HideAllPanels()
    {
        if (hudRoot != null) hudRoot.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    private void UpdateLivesUI()
    {
        if (livesText != null && gameStateManager != null)
        {
            livesText.text = $"Lives: {gameStateManager.GetLives()}";
        }
    }

    // Main menu buttons
    public void StartGame()
    {
        settingsOpen = false;

        // Plays sound when start button is clicked
        if (gameStartAudio != null)
            gameStartAudio.Play();

        if (gameStateManager != null)
            gameStateManager.PressStart();
    }

    public void OpenSettings()
    {
        settingsOpen = true;
        RefreshUI();
    }

    public void CloseSettings()
    {
        settingsOpen = false;
        RefreshUI();
    }

    public void QuitGame()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }

    // Pause / Game Over / Win buttons
    public void ResumeGame()
    {
        if (gameStateManager != null)
            gameStateManager.PressResume();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;

        settingsOpen = false;

        if (gameStateManager != null)
            gameStateManager.PressQuitToMenu();
    }
}