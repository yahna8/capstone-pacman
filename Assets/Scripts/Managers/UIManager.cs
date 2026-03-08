using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Screens / Roots")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject pauseMenuRoot;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        var managers = FindObjectsByType<UIManager>(FindObjectsSortMode.None);

        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Default state on play
        ShowHUD();
        HidePauseMenu();
    }

    public void ShowHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(true);
    }

    public void HideHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        if (paused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }
}
