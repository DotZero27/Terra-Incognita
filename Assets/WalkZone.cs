using UnityEngine;

public class WalkZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.inWalkZone = true;
                playerMovement.moveSpeed = playerMovement.moveSpeed / 2;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.inWalkZone = false;
                playerMovement.moveSpeed = playerMovement.moveSpeed * 2;
            }
        }
    }
}