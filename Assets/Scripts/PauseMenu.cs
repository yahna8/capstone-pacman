using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("Pause Behavior (not GameManager-linked yet)")]
    [SerializeField] private bool pauseTimeScale = true;

    private bool isOpen;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Close);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevelPlaceholder);
        if (quitButton != null) quitButton.onClick.AddListener(QuitPlaceholder);
    }

    private void OnDestroy()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(Close);
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartLevelPlaceholder);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitPlaceholder);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) Close();
            else Open();
        }
    }

    public void Open()
    {
        isOpen = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseTimeScale)
            Time.timeScale = 0f;

        // unlock cursor for desktop testing
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        isOpen = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseTimeScale)
            Time.timeScale = 1f;

        // re-lock cursor for desktop testing
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestartLevelPlaceholder()
    {
        Debug.Log("Restart clicked (placeholder). Hook to GameManager later.");
    }

    private void QuitPlaceholder()
    {
        Debug.Log("Quit clicked (placeholder). Hook to GameManager later.");
        // Application.Quit(); // Leave commented for now
    }
}
