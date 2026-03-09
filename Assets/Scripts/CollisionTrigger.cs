using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    public ScoreManager scoreManager;
    public GameStateManager gameStateManager;

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

        if (hit.collider.CompareTag("Ghost"))
        {
            if (gameStateManager != null)
            {
                gameStateManager.NotifyPlayerDied();
            }

            return;
        }
    }
}

