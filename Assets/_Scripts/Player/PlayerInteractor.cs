using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Transform interactorSource;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private bool showDebugGizmos = true;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject interactionUI;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 18f;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private readonly HashSet<IInteractable> interactablesInRange = new HashSet<IInteractable>();
    private IInteractable currentTarget;
    private SphereCollider detectionCollider;
    private bool isRotating = false;
    private float rotationStartTime;
    private const float ROTATION_DURATION = 0.5f;

    private void Awake()
    {
        InitializeInteractorSource();
        SetupComponents();
    }

    private void InitializeInteractorSource()
    {
        if (interactorSource == null)
        {
            GameObject source = new GameObject("InteractorSource");
            source.transform.SetParent(transform);
            source.transform.localPosition = new Vector3(0, 0, 1f);
            interactorSource = source.transform;
        }
    }

    private void SetupComponents()
    {
        // Setup detection trigger
        detectionCollider = interactorSource.gameObject.AddComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = interactionRadius;

        // Setup input
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void OnEnable()
    {
        interactAction.performed += DoInteract;
    }

    private void OnDisable()
    {
        interactAction.performed -= DoInteract;
        HideInteractionUI();
    }

    private void Update()
    {
        UpdateInteraction();

        if (isRotating && currentTarget != null)
        {
            RotateTowardsTarget();

            if (Time.time - rotationStartTime > ROTATION_DURATION)
            {
                isRotating = false;
            }
        }
    }

    private void RotateTowardsTarget()
    {
        MonoBehaviour targetObject = currentTarget as MonoBehaviour;
        if (targetObject == null) return;

        Vector3 directionToTarget = targetObject.transform.position - transform.position;
        directionToTarget.y = 0; // Keep the rotation only on the Y axis

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateInteraction()
    {
        // Clean up any null references
        interactablesInRange.RemoveWhere(x => x == null);

        if (interactablesInRange.Count == 0)
        {
            SetCurrentTarget(null);
            return;
        }

        // Find closest interactable
        IInteractable closest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (var interactable in interactablesInRange)
        {
            if (interactable == null) continue;

            var interactableObject = interactable as MonoBehaviour;
            if (interactableObject == null) continue;

            float distanceSqr = (interactableObject.transform.position - interactorSource.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = interactable;
            }
        }

        SetCurrentTarget(closest);
    }

    private void SetCurrentTarget(IInteractable newTarget)
    {
        if (currentTarget == newTarget) return;

        currentTarget = newTarget;

        if (currentTarget != null)
        {
            ShowInteractionUI("Press E to Interact");
        }
        else
        {
            HideInteractionUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
        {
            interactable = other.GetComponentInParent<IInteractable>();
        }

        if (interactable != null)
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
        {
            interactable = other.GetComponentInParent<IInteractable>();
        }

        if (interactable != null)
        {
            interactablesInRange.Remove(interactable);

            if (interactable == currentTarget)
            {
                UpdateInteraction();
            }
        }
    }

    private void ShowInteractionUI(string promptText)
    {
        if (interactionUI != null && interactionText != null)
        {
            interactionUI.SetActive(true);
            interactionText.text = promptText;
        }
    }

    private void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void DoInteract(InputAction.CallbackContext context)
    {
        if (currentTarget != null)
        {
            isRotating = true;
            rotationStartTime = Time.time;
            currentTarget.Interact();
        }
    }

    public void SetInteractionRadius(float newRadius)
    {
        interactionRadius = newRadius;
        if (detectionCollider != null)
        {
            detectionCollider.radius = newRadius;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        if (interactorSource != null)
        {
            // Draw interaction radius
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(interactorSource.position, interactionRadius);

            // In play mode, draw lines to interactables
            if (Application.isPlaying)
            {
                foreach (var interactable in interactablesInRange)
                {
                    if (interactable == null) continue;
                    var interactableObject = interactable as MonoBehaviour;
                    if (interactableObject == null) continue;

                    if (interactable == currentTarget)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(interactorSource.position, interactableObject.transform.position);
                        Gizmos.DrawWireSphere(interactableObject.transform.position, 0.3f);
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(interactorSource.position, interactableObject.transform.position);
                    }
                }
            }
        }
    }
#endif
}