using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    [Tooltip("Drag the destination exit point here.")]
    public Transform exitPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            
            if (cc != null)
            {
                // The CharacterController must be disabled to forcibly change coordinates
                cc.enabled = false;
                cc.transform.position = exitPoint.position;
                cc.enabled = true;
            }
        }
    }
}