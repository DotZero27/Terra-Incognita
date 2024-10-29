using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class Player : MonoBehaviour, IPlayerState
{
    public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; }
    public bool isAttacking { get; set; } = false;
    public bool isDashing { get; set; } = false;

    [HideInInspector]
    public Rigidbody playerRigidbody;
    [HideInInspector]
    public PlayerInput playerInput;
    public TrailRenderer trailRenderer;
    [HideInInspector]
    public Animator anim;
    public CinemachineVirtualCamera cinemachineCamera;

    [HideInInspector]
    public InputAction movement;
    [HideInInspector]
    public InputAction dashAction;
    [HideInInspector]
    public InputAction attackAction;


    void Awake()
    {
        ToggleChildComponent(false);

        CurrentHealth = MaxHealth;
        playerRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();

        // Ensure trailRenderer is assigned
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        else
        {
            Debug.LogWarning("TrailRenderer not assigned in the Inspector.");
        }

        if (playerInput != null)
        {
            movement = playerInput.actions["Walk"];
            dashAction = playerInput.actions["Dash"];
            attackAction = playerInput.actions["Attack"];
        }

        ToggleChildComponent(true);
    }
    private void ToggleChildComponent(bool enable = false)
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        PlayerCombat playerCombat = GetComponent<PlayerCombat>();


        if (playerMovement != null)
        {
            playerMovement.enabled = enable;
        }
        else
        {
            Debug.LogWarning("PlayerMovement Component not initialized");
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = enable;
        }
        else
        {
            Debug.LogWarning("PlayerCombat Component not initialized");
        }
    }



    #region IDamageable Implementation
    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Player died");
    }
    #endregion
}