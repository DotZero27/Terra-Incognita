using UnityEngine;

public abstract class NPC : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] protected DialogueContoller dialogueController;

    [Header("Settings")]
    [SerializeField] protected float maxInteractionDistance = 5f;
    [SerializeField] protected float rotationSpeed = 5f;

    protected Transform playerTransform;
    protected bool isInteracting;
    protected bool isLookingAtPlayer;

    protected virtual void Start()
    {
        // Find player by tag
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError($"No object with 'Player' tag found in scene for {gameObject.name}!");
        }

        // Validate DialogueController reference
        if (dialogueController == null)
        {
            Debug.LogError($"DialogueController reference missing on {gameObject.name}. Please assign it in the inspector!");
        }
    }

    protected virtual void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (isInteracting && distance > maxInteractionDistance)
            {
                EndInteraction();
            }
            
            if (distance <= maxInteractionDistance || isInteracting)
            {
                LookAtPlayer();
            }
        }
    }

    protected virtual void LookAtPlayer()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    protected virtual void EndInteraction()
    {
        isInteracting = false;
        if (dialogueController != null)
        {
            dialogueController.ForceEndConversation();
        }
    }

    public abstract void Interact();
}