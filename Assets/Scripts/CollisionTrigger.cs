using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameStateManager gameStateManager;

    [Header("Ghost Collision")]
    [SerializeField] private float collisionRepeatGuardSeconds = 0.2f;

    private int lastGhostCollisionId = int.MinValue;
    private float lastGhostCollisionTime = -999f;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance;

        if (gameStateManager == null)
            gameStateManager = FindAnyObjectByType<GameStateManager>();
    }

    // PELLETS & POWER PELLETS (Trigger-based)
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Pellet"))
        {
            scoreManager?.NotifyPelletConsumed(isPowerPellet: false);
            other.enabled = false;
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("PowerPellet"))
        {
            scoreManager?.NotifyPelletConsumed(isPowerPellet: true);
            other.enabled = false;
            Destroy(other.gameObject);
            return;
        }
    }

    // GHOSTS (CharacterController collision)
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GhostStateManager ghostManager = hit.collider.GetComponentInParent<GhostStateManager>();
        bool isGhostTag = hit.collider.CompareTag("Ghost");
        if (ghostManager == null && !isGhostTag)
            return;

        if (gameStateManager != null && !gameStateManager.IsInState(gameStateManager.PlayingState))
            return;

        int collisionId = ghostManager != null ? ghostManager.GetInstanceID() : hit.collider.GetInstanceID();
        if (IsRepeatCollision(collisionId))
            return;

        if (ghostManager != null)
        {
            if (ghostManager.TryBecomeEatenFromPlayerCollision())
            {
                scoreManager?.NotifyGhostEaten();
                MarkCollisionHandled(collisionId);
                return;
            }

            if (ghostManager.IsDangerousToPlayerOnCollision())
            {
                gameStateManager?.NotifyPlayerDied();
                MarkCollisionHandled(collisionId);
            }
            return;
        }

        // Fallback for legacy ghosts without GhostStateManager.
        if (isGhostTag)
        {
            gameStateManager?.NotifyPlayerDied();
            MarkCollisionHandled(collisionId);
        }
    }

    private bool IsRepeatCollision(int collisionId)
    {
        if (collisionRepeatGuardSeconds <= 0f)
            return false;

        return collisionId == lastGhostCollisionId &&
               Time.time < lastGhostCollisionTime + collisionRepeatGuardSeconds;
    }

    private void MarkCollisionHandled(int collisionId)
    {
        lastGhostCollisionId = collisionId;
        lastGhostCollisionTime = Time.time;
    }
}

