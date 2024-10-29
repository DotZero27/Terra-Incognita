using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    private Animator animator;
    private PlayerInput playerInput;

    private InputAction attackAction;

    [Header("Animation")]
    private string currentState;
    private static readonly string ATTACK_STATE = "Slash";

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    private bool canAttack = true;
    private float lastAttackTime;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        attackAction = playerInput.actions["Attack"];
    }

    private void OnEnable()
    {
        attackAction.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (canAttack)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        ChangeAnimationState(ATTACK_STATE);
        canAttack = false;
        lastAttackTime = Time.time;

        // Add additional attack logic here (e.g., damage calculation, hit detection)
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    void Update()
    {
        // Check if enough time has passed since the last attack
        if (!canAttack && Time.time - lastAttackTime >= attackCooldown)
        {
            canAttack = true;
        }
    }
}