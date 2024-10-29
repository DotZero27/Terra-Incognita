using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private Player player;
    public List<AttackSO> combo;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    private bool wasInAttackState = false;

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 1.5f; // Time after which combo resets if no attack

    [SerializeField] Weapon weapon;

    [Header("Aim Assist Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float aimAssistAngle = 120f;

    private IDamageable currentTarget;
    private Transform currentTargetTransform;
    private Vector3 moveDirection;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {

        UpdateFacingDirection();

        if (player.isAttacking && currentTarget != null)
        {
            RotateTowardsTarget();
        }
    }

    private void Update()
    {
        UpdateAttackState();

        if (comboCounter > 0 && Time.time - lastClickedTime > comboResetTime)
        {
            EndCombo();
        }
    }

    private void UpdateFacingDirection()
    {
        Vector2 input = player.movement.ReadValue<Vector2>();

        if (input.magnitude > 0.1f && !player.isAttacking)
        {
            moveDirection = PlayerUtilities.Movement.InputToDirection(input);
            Vector3 adjustedDirection = PlayerUtilities.Movement.AdjustDirectionToCamera(
                moveDirection,
                player.cinemachineCamera.VirtualCameraGameObject.transform
            );

            Quaternion targetRotation = PlayerUtilities.Rotation.GetCardinalRotation(adjustedDirection);
            PlayerUtilities.Rotation.RotateTowards(transform, targetRotation, rotationSpeed);
        }
    }

    private IDamageable FindNearestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        IDamageable nearest = null;
        float nearestDistance = float.MaxValue;
        Transform nearestTransform = null;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable == null) continue;

            Vector3 directionToTarget = (hitCollider.transform.position - transform.position);
            directionToTarget.y = 0;
            directionToTarget.Normalize();

            Vector3 checkDirection = moveDirection.magnitude > 0.1f ?
                PlayerUtilities.Movement.AdjustDirectionToCamera(
                    moveDirection,
                    player.cinemachineCamera.VirtualCameraGameObject.transform
                ) :
                transform.forward;

            float angle = Vector3.Angle(checkDirection, directionToTarget);

            if (angle <= aimAssistAngle * 0.5f)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = damageable;
                    nearestTransform = hitCollider.transform;
                }
            }
        }

        if (nearest != null)
        {
            currentTargetTransform = nearestTransform;
        }

        return nearest;
    }

    private void RotateTowardsTarget()
    {
        if (currentTargetTransform != null)
        {
            PlayerUtilities.Rotation.RotateTowardsPosition(transform, currentTargetTransform.position, rotationSpeed);
        }
    }

    private void OnEnable()
    {
        player.attackAction.performed += OnAttackPerformed;
        player.attackAction.canceled += OnAttackCancelled;
    }

    private void OnDisable()
    {
        player.attackAction.canceled -= OnAttackCancelled;
        player.attackAction.canceled -= OnAttackCancelled;
    }

    private void UpdateAttackState()
    {
        bool isInAttackAnimation = IsInAttackAnimation();

        if (isInAttackAnimation && !wasInAttackState)
        {
            OnAttackStart();
        }
        else if (!isInAttackAnimation && wasInAttackState)
        {
            OnAttackEnd();
        }

        wasInAttackState = isInAttackAnimation;
    }

    private bool IsInAttackAnimation()
    {
        return player.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
    }

    private void OnAttackStart()
    {
        player.isAttacking = true;
        player.anim.applyRootMotion = true;
        currentTarget = FindNearestEnemy();
    }

    private void OnAttackEnd()
    {
        player.isAttacking = false;
        player.anim.applyRootMotion = false;
        weapon.DisableTriggerBox();

        // Only schedule combo end if we're actually ending an attack
        // not if we're being called during a dash
        if (!player.isDashing)
        {
            ScheduleComboEnd();
        }

        currentTarget = null;
        currentTargetTransform = null;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Attack();
    }

    private void OnAttackCancelled(InputAction.CallbackContext context)
    {
    }

    void Attack()
    {
        // Simplified check - remove the timing check since we handle combo timing elsewhere
        if (comboCounter < combo.Count && !player.isDashing)
        {
            CancelInvoke("EndCombo");

            if (PlayerUtilities.Timing.CheckCooldown(lastClickedTime, 0.4f))
            {
                AttackSO currentAttack = combo[comboCounter];

                weapon.EnableTriggerBox();
                player.anim.runtimeAnimatorController = currentAttack.animatorOV;
                player.anim.SetFloat("AttackSpeed", currentAttack.attackSpeed);
                player.anim.Play("Attack", 0, 0);
                weapon.damage = currentAttack.damage;

                comboCounter++;
                lastClickedTime = Time.time;

                if (comboCounter + 1 > combo.Count)
                {
                    comboCounter = 0;
                }
            }
        }
    }

    private void ScheduleComboEnd()
    {
        if (player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.2f)
        {
            Invoke(nameof(EndCombo), 0.44f);
        }
    }

    void EndCombo()
    {
        comboCounter = 0;
        lastComboEnd = Time.time;
        player.anim.runtimeAnimatorController = combo[0].animatorOV;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (Application.isPlaying)
        {
            // Draw the facing direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * detectionRadius);

            // Draw the aim assist angle
            Gizmos.color = Color.red;
            Vector3 rightDir = Quaternion.Euler(0, aimAssistAngle * 0.5f, 0) * transform.forward;
            Vector3 leftDir = Quaternion.Euler(0, -aimAssistAngle * 0.5f, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, rightDir * detectionRadius);
            Gizmos.DrawRay(transform.position, leftDir * detectionRadius);
        }
    }
}