using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    public ScoreManager scoreManager;
    public GameStateManager gameStateManager;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance != null ? ScoreManager.Instance : FindAnyObjectByType<ScoreManager>();

        if (gameStateManager == null)
            gameStateManager = FindAnyObjectByType<GameStateManager>();
    }

    // PELLETS & POWER PELLETS (Trigger-based)
    private void OnTriggerEnter(Collider other)
    {
        TryConsumePellet(other);
    }

    // GHOSTS (CharacterController collision)
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (TryConsumePellet(hit.collider))
            return;

        if (hit.collider.CompareTag("Ghost"))
        {
            GhostStateManager ghost = hit.collider.GetComponentInParent<GhostStateManager>();
            if (ghost != null && ghost.TryBecomeEatenFromPlayerCollision())
            {
                return;
            }

            bool gameIsPlaying = gameStateManager == null || gameStateManager.IsPlaying();
            if (gameIsPlaying && (ghost == null || ghost.IsDangerousToPlayerOnCollision()))
            {
                gameStateManager?.NotifyPlayerDied();
            }

            return;
        }
    }

    private bool TryConsumePellet(Collider other)
    {
        if (!TryGetPelletObject(other, out bool isPowerPellet, out GameObject pelletObject))
            return false;

        scoreManager?.NotifyPelletConsumed(isPowerPellet);

        Collider[] colliders = pelletObject.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        Destroy(pelletObject);
        return true;
    }

    private bool TryGetPelletObject(Collider other, out bool isPowerPellet, out GameObject pelletObject)
    {
        isPowerPellet = false;
        pelletObject = null;

        if (other == null)
            return false;

        Transform current = other.transform;
        while (current != null)
        {
            if (current.CompareTag("PowerPellet"))
            {
                isPowerPellet = true;
                pelletObject = current.gameObject;
                return true;
            }

            if (current.CompareTag("Pellet"))
            {
                pelletObject = current.gameObject;
                return true;
            }

            if (current.gameObject.name.Contains("PowerPellet"))
            {
                isPowerPellet = true;
                pelletObject = current.gameObject;
                return true;
            }

            if (current.gameObject.name.Contains("Pellet"))
            {
                pelletObject = current.gameObject;
                return true;
            }

            current = current.parent;
        }

        string objectName = other.gameObject.name;
        if (objectName.Contains("PowerPellet"))
        {
            isPowerPellet = true;
            pelletObject = other.gameObject;
            return true;
        }

        if (objectName.Contains("Pellet"))
        {
            pelletObject = other.gameObject;
            return true;
        }

        return false;
    }
}
