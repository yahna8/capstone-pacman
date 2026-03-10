using UnityEngine;
using System;

public enum GhostPatrolMode
{
    Scatter,
    Chase
}

[DefaultExecutionOrder(-100)]
public class GhostModeController : MonoBehaviour
{
    public static GhostModeController Instance { get; private set; }
    public event Action<GhostPatrolMode> ModeChanged;

    [Header("Cycle Durations (seconds)")]
    [SerializeField] private float scatterSeconds = 7f;
    [SerializeField] private float chaseSeconds = 20f;

    [Header("Mode Cycle")]
    [SerializeField] private bool startInScatter = true;
    [SerializeField] private bool runCycle = true;

    [Header("Debug (runtime)")]
    [SerializeField] private GhostPatrolMode currentMode;
    [SerializeField] private float modeTimer;

    public GhostPatrolMode CurrentMode => currentMode;

    public static GhostModeController GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GhostModeController existing = FindAnyObjectByType<GhostModeController>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("GhostModeController_Auto");
        return go.AddComponent<GhostModeController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        ResetCycle();
    }

    private void Update()
    {
        if (!runCycle)
            return;

        modeTimer += Time.deltaTime;
        float duration = currentMode == GhostPatrolMode.Scatter ? scatterSeconds : chaseSeconds;

        if (duration <= 0f || modeTimer >= duration)
            ToggleMode();
    }

    public void ResetCycle()
    {
        SetMode(startInScatter ? GhostPatrolMode.Scatter : GhostPatrolMode.Chase);
    }

    public void ForceScatter()
    {
        SetMode(GhostPatrolMode.Scatter);
    }

    public void ForceChase()
    {
        SetMode(GhostPatrolMode.Chase);
    }

    private void ToggleMode()
    {
        GhostPatrolMode next = currentMode == GhostPatrolMode.Scatter
            ? GhostPatrolMode.Chase
            : GhostPatrolMode.Scatter;
        SetMode(next);
    }

    private void SetMode(GhostPatrolMode mode)
    {
        currentMode = mode;
        modeTimer = 0f;
        ModeChanged?.Invoke(currentMode);
    }
}
