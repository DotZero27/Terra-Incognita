using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private Vector3 rootMotionDelta;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rootMotionMultiplier = 2f;
    public float rotationSpeed = 18f;

    [Header("Animation Control")]
    public bool inWalkZone = false;  // New variable to control animation state

    private Vector3 moveVector = Vector3.zero;
    private Vector3 lastMoveVector = Vector3.zero;
    private bool isMoving = false;

    [Header("Dash")]
    public float dashForce = 45f;
    public float dashDuration = 0.05f;
    public float dashCooldown = 0.95f;
    [HideInInspector]
    public Vector3 dashDirection;
    private bool canDash = true;
    private float dashTimeLeft = 0f;
    private float dashCooldownTimeLeft = 0f;

    // Animation parameter hashes for better performance
    private readonly int RunningSpeedParam = Animator.StringToHash("RunningSpeed");
    private readonly int DashParam = Animator.StringToHash("Dash");

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.movement.performed += OnMovementPerformed;
        player.movement.canceled += OnMovementCancelled;
        player.dashAction.performed += OnDashPerformed;
    }

    private void OnDisable()
    {
        player.movement.performed -= OnMovementPerformed;
        player.movement.canceled -= OnMovementCancelled;
        player.dashAction.performed -= OnDashPerformed;
    }

    private void OnAnimatorMove()
    {
        if (player.isAttacking && player.anim.applyRootMotion)
        {
            rootMotionDelta = player.anim.deltaPosition * rootMotionMultiplier;
            Vector3 rootMotionTargetPosition = player.playerRigidbody.position + rootMotionDelta;

            player.playerRigidbody.MovePosition(
                new Vector3(rootMotionTargetPosition.x, player.playerRigidbody.position.y, rootMotionTargetPosition.z)
            );
        }
    }

    private void FixedUpdate()
    {
        if (player.isAttacking && player.anim.applyRootMotion)
        {
            return;
        }

        ApplyDash();

        if (lastMoveVector == Vector3.zero)
        {
            lastMoveVector = moveVector;
        }

        if (!player.isDashing && !player.isAttacking)
        {
            float currentYVelocity = player.playerRigidbody.velocity.y;

            Vector3 adjustedMoveVector = PlayerUtilities.Movement.AdjustDirectionToCamera(
                moveVector,
                player.cinemachineCamera.VirtualCameraGameObject.transform
            );

            Vector3 newVelocity = new Vector3(
                adjustedMoveVector.x * moveSpeed,
                currentYVelocity,
                adjustedMoveVector.z * moveSpeed
            );
            player.playerRigidbody.velocity = newVelocity;

            if (isMoving && moveVector != Vector3.zero)
            {
                Quaternion targetRotation = PlayerUtilities.Rotation.GetCardinalRotation(adjustedMoveVector);
                PlayerUtilities.Rotation.RotateTowards(transform, targetRotation, rotationSpeed);
            }

            if (lastMoveVector != Vector3.zero)
            {
                moveVector = lastMoveVector;
                lastMoveVector = Vector3.zero;
            }
        }
        else
        {
            moveVector = Vector3.zero;
        }
    }

    private void Update()
    {
        float currentSpeed = Mathf.Abs(moveVector.x) + Mathf.Abs(moveVector.z);

        // If in walk zone, cap the speed at 0.5 but allow idle
        if (inWalkZone)
        {
            currentSpeed = Mathf.Min(currentSpeed, 0.5f);
        }

        player.anim.SetFloat(RunningSpeedParam, currentSpeed);
        player.anim.SetBool(DashParam, player.isDashing);
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        isMoving = true;
        Vector2 inputVector = context.ReadValue<Vector2>();
        moveVector = PlayerUtilities.Movement.InputToDirection(inputVector);

        if (!player.isAttacking && !player.isDashing)
        {
            lastMoveVector = Vector3.zero;
        }
        else
        {
            lastMoveVector = moveVector;
        }
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        isMoving = false;
        moveVector = Vector3.zero;

        if (!player.isAttacking && !player.isDashing)
        {
            lastMoveVector = Vector3.zero;
        }
        else
        {
            lastMoveVector = moveVector;
        }
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (canDash && moveVector != Vector3.zero)
        {
            player.isDashing = true;
            canDash = false;

            Vector3 currentVector = moveVector;
            dashDirection = PlayerUtilities.Movement.AdjustDirectionToCamera(
                currentVector,
                player.cinemachineCamera.VirtualCameraGameObject.transform
            );

            dashTimeLeft = dashDuration;
            dashCooldownTimeLeft = dashCooldown;

            player.trailRenderer.emitting = true;
        }
    }

    private void ApplyDash()
    {
        if (player.isDashing)
        {
            if (dashTimeLeft > 0)
            {
                player.playerRigidbody.velocity = dashDirection * dashForce;
                dashTimeLeft -= Time.fixedDeltaTime;
            }
            else
            {
                player.isDashing = false;
                if (player.trailRenderer != null)
                {
                    player.trailRenderer.emitting = false;
                }
            }
        }

        if (!canDash)
        {
            if (dashCooldownTimeLeft > 0)
            {
                dashCooldownTimeLeft -= Time.fixedDeltaTime;
            }
            else
            {
                canDash = true;
            }
        }
    }
}