using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GhostStateManager : MonoBehaviour
{
    public enum GhostType
    {
        Blinky,
        Pinky,
        Inky,
        Clyde,
        Custom
    }

    private GhostBaseState currentState;

    // States
    public GhostSpawnState SpawnState = new GhostSpawnState();
    public GhostChaseState ChaseState = new GhostChaseState();
    public GhostScatterState ScatterState = new GhostScatterState();
    public GhostFrightenedState FrightenedState = new GhostFrightenedState();
    public GhostEatenState EatenState = new GhostEatenState();

    [Header("References")]
    [SerializeField] private GhostType ghostType = GhostType.Blinky;
    [SerializeField] private GhostModeController modeController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform scatterCornerTransform;
    [SerializeField] private Transform homeTransform;
    [SerializeField] private Transform spawnTransform;

    [Header("Speeds (m/s)")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float frightenedSpeed = 2.0f;
    [SerializeField] private float returnSpeed = 4.0f;

    [Header("Movement")]
    [SerializeField] private float turnSmooth = 12f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private float frightenedRetreatDistance = 3f;

    [Header("Classic Chase Tuning")]
    [SerializeField] private float pinkyLookAheadDistance = 4f;
    [SerializeField] private float inkyLookAheadDistance = 2f;
    [SerializeField] private float clydeScatterDistance = 8f;
    [SerializeField] private float playerHeadingVelocityThreshold = 0.1f;

    [Header("Debug State Forces (temporary)")]
    public bool ForceSpawn;
    public bool ForceChase;
    public bool ForceScatter;
    public bool ForceFrightened;
    public bool ForceEaten;

    [Header("Debug Respawn (temporary)")]
    [SerializeField] private bool forceRespawn;

    [Header("Mode Control")]
    [SerializeField] private bool useTimedPatrolModes = true;

    [Header("Transition Flags (temporary)")]
    public bool SpawnReached;
    public bool ShouldEnterChase;
    public bool ShouldEnterScatter;
    public bool IsFrightened;
    public bool IsEaten;
    public bool ReachedHome;

    [Header("Timers (temporary)")]
    public float FrightenedSeconds = 6f;
    [HideInInspector] public float FrightenedTimer;

    [Header("Debug Movement (runtime)")]
    [SerializeField] private bool hasMoveTarget;
    [SerializeField] private Vector3 currentMoveTarget;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private bool isPowerPelletSubscribed;
    private GhostBaseState patrolStateBeforeFrightened;
    private bool hasPatrolStateBeforeFrightened;
    private static readonly List<GhostStateManager> ActiveGhosts = new List<GhostStateManager>();

    public GhostType Type => ghostType;
    public GhostModeController ModeController => modeController;
    public Transform PlayerTransform => playerTransform;
    public Transform ScatterCornerTransform => scatterCornerTransform;
    public Transform HomeTransform => homeTransform;
    public Transform SpawnTransform => spawnTransform;
    public float ChaseSpeed => chaseSpeed;
    public float FrightenedSpeed => frightenedSpeed;
    public float ReturnSpeed => returnSpeed;
    public float FrightenedRetreatDistance => frightenedRetreatDistance;

    public static Vector3 GetSpawnOffsetForType(GhostType type)
    {
        switch (type)
        {
            case GhostType.Blinky:
                return new Vector3(0f, 0f, 0.45f);
            case GhostType.Pinky:
                return new Vector3(-0.45f, 0f, 0f);
            case GhostType.Inky:
                return new Vector3(0.45f, 0f, 0f);
            case GhostType.Clyde:
                return new Vector3(0f, 0f, -0.45f);
            case GhostType.Custom:
            default:
                return Vector3.zero;
        }
    }

    public void SetGhostType(GhostType type)
    {
        ghostType = type;
    }

    public Vector3 GetSpawnTargetPosition()
    {
        if (spawnTransform == null)
            return transform.position;

        return spawnTransform.position + GetSpawnOffsetForType(ghostType);
    }

    public void ConfigureRuntimeReferences(
        Transform player,
        Transform scatterCorner,
        Transform home,
        Transform spawn,
        ScoreManager score,
        GhostModeController mode)
    {
        if (player != null)
            playerTransform = player;

        if (scatterCorner != null)
            scatterCornerTransform = scatterCorner;

        if (home != null)
            homeTransform = home;

        if (spawn != null)
            spawnTransform = spawn;

        if (mode != null && modeController != mode)
        {
            if (modeController != null)
                modeController.ModeChanged -= HandlePatrolModeChanged;

            modeController = mode;

            if (isActiveAndEnabled)
                modeController.ModeChanged += HandlePatrolModeChanged;
        }

        if (score != null && scoreManager != score)
        {
            if (isPowerPelletSubscribed && scoreManager != null)
                scoreManager.OnPowerPelletConsumed -= HandlePowerPelletConsumed;

            scoreManager = score;
            isPowerPelletSubscribed = false;

            if (isActiveAndEnabled)
                TrySubscribeToPowerPelletEvent();
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (modeController == null)
            modeController = GhostModeController.GetOrCreate();

        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;
    }

    private void OnEnable()
    {
        RegisterGhost(this);

        if (modeController == null)
            modeController = GhostModeController.GetOrCreate();

        if (modeController != null)
            modeController.ModeChanged += HandlePatrolModeChanged;

        TrySubscribeToPowerPelletEvent();
    }

    private void OnDisable()
    {
        UnregisterGhost(this);

        if (modeController != null)
            modeController.ModeChanged -= HandlePatrolModeChanged;

        UnsubscribeFromPowerPelletEvent();
    }

    private void Start()
    {
        TrySubscribeToPowerPelletEvent();
        currentState = SpawnState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        if (!isPowerPelletSubscribed)
            TrySubscribeToPowerPelletEvent();

        if (forceRespawn)
        {
            forceRespawn = false;
            ForceRespawn();
        }

        DebugHotkeys();
        HandleForcedTransitions();
        SyncTimedPatrolMode();
        currentState.UpdateState(this);
    }

    public void SwitchState(GhostBaseState state)
    {
        if (state == null || state == currentState)
            return;

        currentState?.ExitState(this);
        currentState = state;
        currentState.EnterState(this);
    }

    public bool IsInState(GhostBaseState state)
    {
        return currentState == state;
    }

    public GhostBaseState GetCurrentState()
    {
        return currentState;
    }

    public GhostBaseState GetTimedPatrolState()
    {
        if (!useTimedPatrolModes || modeController == null)
            return ScatterState;

        return modeController.CurrentMode == GhostPatrolMode.Chase
            ? (GhostBaseState)ChaseState
            : ScatterState;
    }

    public GhostBaseState GetPostFrightenedState()
    {
        if (hasPatrolStateBeforeFrightened &&
            (patrolStateBeforeFrightened == ChaseState || patrolStateBeforeFrightened == ScatterState))
        {
            return patrolStateBeforeFrightened;
        }

        return GetTimedPatrolState();
    }

    public GhostBaseState GetPostFrightenedStateAndSyncMode()
    {
        GhostBaseState postState = GetPostFrightenedState();

        if (useTimedPatrolModes && modeController != null)
        {
            if (postState == ChaseState)
                modeController.ForceChase();
            else if (postState == ScatterState)
                modeController.ForceScatter();
        }

        return postState;
    }

    public void ClearPostFrightenedState()
    {
        hasPatrolStateBeforeFrightened = false;
        patrolStateBeforeFrightened = null;
    }

    [ContextMenu("Force Respawn")]
    public void ForceRespawn()
    {
        Vector3 target = transform.position;
        if (homeTransform != null)
            target = homeTransform.position;
        else if (spawnTransform != null)
            target = spawnTransform.position;

        TeleportTo(target);

        // Clear transient flags so Spawn starts cleanly.
        SpawnReached = false;
        ReachedHome = false;
        IsEaten = false;
        IsFrightened = false;
        ShouldEnterChase = false;
        ShouldEnterScatter = false;
        ClearPostFrightenedState();

        SwitchState(SpawnState);
    }

    public void ResetToHomeAndRespawn()
    {
        ForceRespawn();
    }

    public void TriggerFrightenedFromPowerPellet()
    {
        if (currentState == EatenState || IsEaten)
            return;

        if (currentState == FrightenedState)
        {
            FrightenedTimer = FrightenedSeconds;
            return;
        }

        CachePatrolStateForFrightenedReturn();
        IsFrightened = true;
        SwitchState(FrightenedState);
    }

    public bool IsDangerousToPlayerOnCollision()
    {
        return currentState == ChaseState || currentState == ScatterState;
    }

    public bool TryBecomeEatenFromPlayerCollision()
    {
        if (currentState != FrightenedState || IsEaten)
            return false;

        IsFrightened = false;
        IsEaten = true;
        ClearPostFrightenedState();
        SwitchState(EatenState);
        return true;
    }

    public void SetMoveTarget(Vector3 worldTarget)
    {
        currentMoveTarget = worldTarget;
        hasMoveTarget = true;
    }

    public bool MoveToward(Vector3 worldTarget, float speed)
    {
        SetMoveTarget(worldTarget);
        return MoveTowardCurrentTarget(speed);
    }

    public Vector3 GetChaseTarget()
    {
        if (playerTransform == null)
            return transform.position;

        Vector3 playerPos = playerTransform.position;
        Vector3 playerHeading = GetPlayerHeadingOnPlane();

        switch (ghostType)
        {
            case GhostType.Blinky:
                return playerPos;

            case GhostType.Pinky:
                return playerPos + playerHeading * pinkyLookAheadDistance;

            case GhostType.Inky:
                return GetInkyTarget(playerPos, playerHeading);

            case GhostType.Clyde:
                return GetClydeTarget(playerPos);

            case GhostType.Custom:
            default:
                return playerPos;
        }
    }

    public bool TryGetScatterTarget(out Vector3 worldTarget)
    {
        if (scatterCornerTransform == null)
        {
            worldTarget = transform.position;
            return false;
        }

        worldTarget = scatterCornerTransform.position;
        return true;
    }

    private bool MoveTowardCurrentTarget(float speed)
    {
        if (!hasMoveTarget)
            return false;

        Vector3 toTarget = currentMoveTarget - transform.position;
        toTarget.y = 0f;

        bool reached = toTarget.sqrMagnitude <= arriveDistance * arriveDistance;
        Vector3 dir = reached ? Vector3.zero : toTarget.normalized;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSmooth * Time.deltaTime);
        }

        if (controller != null)
        {
            controller.Move(dir * speed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity.y < 0f)
                verticalVelocity.y = -2f;

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }
        else
        {
            transform.position += dir * speed * Time.deltaTime;
        }

        return reached;
    }

    private void TeleportTo(Vector3 worldPosition)
    {
        if (controller != null && controller.enabled)
        {
            controller.enabled = false;
            transform.position = worldPosition;
            controller.enabled = true;
        }
        else
        {
            transform.position = worldPosition;
        }

        verticalVelocity = Vector3.zero;
    }

    private void HandleForcedTransitions()
    {
        if (ForceSpawn)
        {
            ForceSpawn = false;
            SwitchState(SpawnState);
            return;
        }

        if (ForceChase)
        {
            ForceChase = false;
            SwitchState(ChaseState);
            return;
        }

        if (ForceScatter)
        {
            ForceScatter = false;
            SwitchState(ScatterState);
            return;
        }

        if (ForceFrightened)
        {
            ForceFrightened = false;
            TriggerFrightenedFromPowerPellet();
            return;
        }

        if (ForceEaten)
        {
            ForceEaten = false;
            IsEaten = true;
            SwitchState(EatenState);
        }
    }

    private void SyncTimedPatrolMode()
    {
        if (!useTimedPatrolModes || modeController == null)
            return;

        // Only auto-cycle while in normal patrol states.
        if (currentState != ChaseState && currentState != ScatterState)
            return;

        GhostBaseState desired = GetTimedPatrolState();
        if (desired != currentState)
            SwitchState(desired);
    }

    private void HandlePatrolModeChanged(GhostPatrolMode mode)
    {
        if (!useTimedPatrolModes)
            return;

        // Only auto-cycle while in normal patrol states.
        if (currentState != ChaseState && currentState != ScatterState)
            return;

        GhostBaseState desired = mode == GhostPatrolMode.Chase
            ? (GhostBaseState)ChaseState
            : ScatterState;

        if (desired != currentState)
            SwitchState(desired);
    }

    private void TrySubscribeToPowerPelletEvent()
    {
        if (isPowerPelletSubscribed)
            return;

        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;

        if (scoreManager == null)
            return;

        scoreManager.OnPowerPelletConsumed += HandlePowerPelletConsumed;
        isPowerPelletSubscribed = true;
    }

    private void UnsubscribeFromPowerPelletEvent()
    {
        if (!isPowerPelletSubscribed)
            return;

        if (scoreManager != null)
            scoreManager.OnPowerPelletConsumed -= HandlePowerPelletConsumed;

        isPowerPelletSubscribed = false;
    }

    private void HandlePowerPelletConsumed()
    {
        TriggerFrightenedFromPowerPellet();
    }

    private void CachePatrolStateForFrightenedReturn()
    {
        if (currentState == ChaseState || currentState == ScatterState)
            patrolStateBeforeFrightened = currentState;
        else
            patrolStateBeforeFrightened = GetTimedPatrolState();

        hasPatrolStateBeforeFrightened = true;
    }

    private Vector3 GetClydeTarget(Vector3 playerPos)
    {
        Vector3 toPlayer = playerPos - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude <= clydeScatterDistance * clydeScatterDistance &&
            TryGetScatterTarget(out Vector3 scatterTarget))
        {
            return scatterTarget;
        }

        return playerPos;
    }

    private Vector3 GetInkyTarget(Vector3 playerPos, Vector3 playerHeading)
    {
        Vector3 pivot = playerPos + playerHeading * inkyLookAheadDistance;
        GhostStateManager blinky = FindBlinkyGhost();
        if (blinky == null || blinky == this)
            return pivot;

        Vector3 fromBlinky = pivot - blinky.transform.position;
        fromBlinky.y = 0f;
        return pivot + fromBlinky;
    }

    private Vector3 GetPlayerHeadingOnPlane()
    {
        if (playerTransform == null)
            return transform.forward;

        Vector3 heading = Vector3.zero;

        CharacterController playerController = playerTransform.GetComponent<CharacterController>();
        if (playerController != null)
            heading = playerController.velocity;

        if (heading.sqrMagnitude <= playerHeadingVelocityThreshold * playerHeadingVelocityThreshold)
        {
            Rigidbody playerBody = playerTransform.GetComponent<Rigidbody>();
            if (playerBody != null)
                heading = playerBody.linearVelocity;
        }

        if (heading.sqrMagnitude <= playerHeadingVelocityThreshold * playerHeadingVelocityThreshold)
            heading = playerTransform.forward;

        heading.y = 0f;
        if (heading.sqrMagnitude < 0.0001f)
            heading = Vector3.forward;

        return heading.normalized;
    }

    private static GhostStateManager FindBlinkyGhost()
    {
        for (int i = ActiveGhosts.Count - 1; i >= 0; i--)
        {
            GhostStateManager ghost = ActiveGhosts[i];
            if (ghost == null)
            {
                ActiveGhosts.RemoveAt(i);
                continue;
            }

            if (ghost.ghostType == GhostType.Blinky)
                return ghost;
        }

        return null;
    }

    private static void RegisterGhost(GhostStateManager ghost)
    {
        if (ghost == null || ActiveGhosts.Contains(ghost))
            return;

        ActiveGhosts.Add(ghost);
    }

    private static void UnregisterGhost(GhostStateManager ghost)
    {
        if (ghost == null)
            return;

        ActiveGhosts.Remove(ghost);
    }

    private void DebugHotkeys()
    {
        // Temporary desktop-only shortcuts for state-machine testing.
        if (Input.GetKeyDown(KeyCode.Alpha7)) ForceSpawn = true;
        if (Input.GetKeyDown(KeyCode.Alpha8)) ForceScatter = true;
        if (Input.GetKeyDown(KeyCode.Alpha9)) ForceChase = true;
        if (Input.GetKeyDown(KeyCode.Alpha0)) ForceFrightened = true;
        if (Input.GetKeyDown(KeyCode.Minus)) ForceEaten = true;
    }
}
