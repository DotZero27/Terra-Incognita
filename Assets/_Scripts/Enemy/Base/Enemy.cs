using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEnemyMoveable, IDamageable, ITriggerCheckable
{
    private Rigidbody[] _ragdollRigidBodies;
    public Animator anim;
    public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; }
    public NavMeshAgent NavMeshAgent { get; private set; }

    public GameObject FloatingTextPrefab;

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector3 knockbackVelocity;

    #region State Machine Variables
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyAttackState AttackState { get; set; }
    public bool IsAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }
    #endregion

    #region ScriptableObject Variables
    [SerializeField] private EnemyIdleSOBase EnemyIdleBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseBase;
    [SerializeField] private EnemyAttackSOBase EnemyAttackBase;

    public EnemyIdleSOBase EnemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseBaseInstance { get; set; }
    public EnemyAttackSOBase EnemyAttackBaseInstance { get; set; }
    #endregion


    [Header("Knockback Settings")]
    [SerializeField] private float knockbackResistance = 0.5f;
    [SerializeField] private float knockbackRecoverySpeed = 2.0f;
    [SerializeField] private float maxKnockbackTime = 0.5f;
    [SerializeField] private float baseKnockbackForce = 0.95f; // Base force for knockback
    [SerializeField] private float damageToForceRatio = 0.5f; // How much damage affects knockback force

    private static readonly int AnimSpeedParam = Animator.StringToHash("Speed");
    private static readonly int AnimKnockbackTrigger = Animator.StringToHash("Knockback");
    private static readonly int AnimKnockbackWalkTrigger = Animator.StringToHash("KnockbackWalk");
    private static readonly int AnimHitTrigger = Animator.StringToHash("Hit");

    private const float WALK_THRESHOLD = 0.5f;
    private const float RUN_THRESHOLD = 1.0f;

    private float baseSpeed;

    private void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();

        anim = GetComponent<Animator>();
        if (NavMeshAgent != null)
        {
            baseSpeed = NavMeshAgent.speed;
        }
        else
        {
            Debug.LogError("NavMeshAgent component not found on the enemy object.");
        }

        EnemyIdleBaseInstance = Instantiate(EnemyIdleBase);
        EnemyChaseBaseInstance = Instantiate(EnemyChaseBase);
        EnemyAttackBaseInstance = Instantiate(EnemyAttackBase);

        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }

    void Start()
    {
        CurrentHealth = MaxHealth;

        EnemyIdleBaseInstance.Initialize(gameObject, this);
        EnemyChaseBaseInstance.Initialize(gameObject, this);
        EnemyAttackBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (CurrentHealth >= 0)
        {
            StateMachine.CurrentEnemyState.FrameUpdate();
            UpdateKnockbackState();
        }
    }

    private void FixedUpdate()
    {
        if (CurrentHealth >= 0)
        {
            StateMachine.CurrentEnemyState.PhysicsUpdate();
            ApplyKnockbackMovement();
        }
    }
    private void UpdateKnockbackState()
    {
        if (!isKnockedBack) return;

        knockbackTimer += Time.deltaTime;
        if (knockbackTimer >= maxKnockbackTime)
        {
            RecoverFromKnockback();
        }
    }

    private void ApplyKnockbackMovement()
    {
        if (!isKnockedBack) return;

        if (NavMeshAgent != null && NavMeshAgent.enabled)
        {
            NavMeshAgent.Move(knockbackVelocity * Time.fixedDeltaTime);
        }

        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackRecoverySpeed * Time.fixedDeltaTime);
    }

    public void ApplyKnockback(Vector3 knockbackDirection, float force)
    {
        if (CurrentHealth <= 0f) return;

        isKnockedBack = true;
        knockbackTimer = 0f;

        knockbackVelocity = knockbackDirection.normalized * (force * (1f - knockbackResistance));

        if (NavMeshAgent != null && NavMeshAgent.enabled)
        {
            NavMeshAgent.isStopped = true;
            NavMeshAgent.ResetPath();

            // Make the enemy face the opposite direction of the knockback
            // transform.rotation = Quaternion.LookRotation(-knockbackDirection);
        }

        if (anim)
        {
            StartCoroutine(ApplyKnockbackLayers());
        }
    }


    private void RecoverFromKnockback()
    {
        isKnockedBack = false;
        knockbackVelocity = Vector3.zero;

        if (NavMeshAgent != null && NavMeshAgent.enabled)
        {
            NavMeshAgent.isStopped = false;
        }

        if (anim)
        {
            StartCoroutine(RecoverKnockbackLayers());
        }
    }


    private IEnumerator ApplyKnockbackLayers()
    {
        float fadeTime = 0.1f;
        float elapsed = 0f;

        // Get initial base layer weight
        float baseStartWeight = anim.GetLayerWeight(0);

        // Fade layers
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            // Fade out base layer
            anim.SetLayerWeight(0, Mathf.Lerp(baseStartWeight, 0f, t));
            // Fade in knockback layers
            anim.SetLayerWeight(1, Mathf.Lerp(0f, 0.7f, t));  // React layer
            anim.SetLayerWeight(2, Mathf.Lerp(0f, 0.5f, t));  // Walk layer

            yield return null;
        }

        // Ensure final weights are set
        anim.SetLayerWeight(0, 0f);
        anim.SetLayerWeight(1, 0.7f);
        anim.SetLayerWeight(2, 1f);

        // Trigger the knockback animations after weights are set
        anim.SetTrigger(AnimKnockbackTrigger);
        anim.SetTrigger(AnimKnockbackWalkTrigger);
    }
    private IEnumerator RecoverKnockbackLayers()
    {
        float fadeTime = 0.2f;
        float elapsed = 0f;

        // Get current layer weights
        float reactStartWeight = anim.GetLayerWeight(1);
        float walkStartWeight = anim.GetLayerWeight(2);

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            // Fade in base layer
            anim.SetLayerWeight(0, Mathf.Lerp(0f, 1f, t));
            // Fade out knockback layers
            anim.SetLayerWeight(1, Mathf.Lerp(reactStartWeight, 0f, t));
            anim.SetLayerWeight(2, Mathf.Lerp(walkStartWeight, 0f, t));

            yield return null;
        }

        // Ensure final weights are set
        anim.SetLayerWeight(0, 1f);
        anim.SetLayerWeight(1, 0f);
        anim.SetLayerWeight(2, 0f);
    }

    #region IEnemyMoveable Implementation
    public void MoveEnemy(Vector3 destination)
    {
        if (isKnockedBack) return;
        if (CurrentHealth <= 0f) return;

        if (NavMeshAgent != null && NavMeshAgent.isActiveAndEnabled)
        {
            bool isChasing = StateMachine.CurrentEnemyState is EnemyChaseState;
            NavMeshAgent.speed = isChasing ? baseSpeed * 1.5f : baseSpeed * 0.5f;

            NavMeshAgent.updateRotation = true;
            NavMeshAgent.angularSpeed = 360f;
            NavMeshAgent.stoppingDistance = 0.1f;
            NavMeshAgent.isStopped = false;

            NavMeshAgent.SetDestination(destination);

            if (anim)
            {
                float speedParam = isChasing ? RUN_THRESHOLD : WALK_THRESHOLD;
                anim.SetFloat(AnimSpeedParam, speedParam);
            }
        }
    }

    public void StopEnemy()
    {
        if (isKnockedBack) return;

        if (NavMeshAgent != null && NavMeshAgent.isActiveAndEnabled)
        {
            NavMeshAgent.isStopped = true;
            NavMeshAgent.ResetPath();
            NavMeshAgent.speed = baseSpeed;

            if (anim)
            {
                anim.SetFloat(AnimSpeedParam, 0f);
            }
        }
    }
    #endregion

    #region IDamageable Implementation
    public void Damage(float damageAmount, Vector3 damageSource)
    {
        if (CurrentHealth <= 0f) return;

        CurrentHealth -= damageAmount;
        CinemachineShake.Instance.ShakeCamera(intensity: damageAmount / 10, frequency: 2f, time: 0.4f);

        // Calculate knockback direction and force
        Vector3 knockbackDirection = (transform.position - damageSource).normalized;
        knockbackDirection.y = 0; // Optional: prevent vertical knockback
        float knockbackForce = baseKnockbackForce + (damageAmount * damageToForceRatio);

        // Apply knockback
        ApplyKnockback(knockbackDirection, knockbackForce);

        // Trigger hit animation
        if (anim)
        {
            // anim.SetTrigger(AnimHitTrigger);
        }

        if (CurrentHealth <= 0f)
        {
            Die();
        }
        else if (FloatingTextPrefab)
        {
            ShowFloatingText(damageAmount);
        }
    }

    public void Damage(float damageAmount)
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Damage(damageAmount, transform.position + randomDirection);
    }

    public void Die()
    {
        Debug.Log("Enemy died");
        EnableRagdoll();
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        
        if (collider != null)
        {
            collider.enabled = false;
        }
        // Implement death logic here
        // Destroy(gameObject, 4f);
    }
    #endregion
    public void TriggerAttackAnimation()
    {
        if (anim)
        {
            // anim.SetTrigger(AnimAttackTrigger);
        }
    }
    void ShowFloatingText(float damageAmount)
    {
        var _floatingText = Instantiate(FloatingTextPrefab, transform.position, Quaternion.identity, transform);
        _floatingText.GetComponent<TextMeshPro>().text = damageAmount.ToString();
    }
    private void EnableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidBodies)
        {
            rigidbody.isKinematic = false;
        }
        if (anim)
        {
            anim.enabled = false;
        }
    }
    private void DisableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidBodies)
        {
            rigidbody.isKinematic = true;
        }
    }

    #region ITriggerCheckable Implementation
    public void SetAggroStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
    }

    public void SetStrikingDistanceBool(bool isWithinStrikingDistance)
    {
        IsWithinStrikingDistance = isWithinStrikingDistance;
    }
    #endregion

    #region Animation Triggers
    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound,
    }
    #endregion
}