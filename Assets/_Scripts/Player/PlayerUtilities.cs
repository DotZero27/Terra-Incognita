using UnityEngine;

public static class PlayerUtilities
{
    public static class Movement
    {
        public static Vector3 AdjustDirectionToCamera(Vector3 inputDirection, Transform cameraTransform)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return inputDirection.x * right + inputDirection.z * forward;
        }

        public static Vector3 InputToDirection(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y).normalized;
        }
    }

    public static class Rotation
    {
        public static Quaternion GetCardinalRotation(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 45.0f) * 45.0f;
            return Quaternion.Euler(0, snappedAngle, 0);
        }

        public static void RotateTowards(Transform transform, Quaternion targetRotation, float rotationSpeed)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        public static void RotateTowardsPosition(Transform transform, Vector3 targetPosition, float rotationSpeed)
        {
            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0;
            directionToTarget.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
            RotateTowards(transform, targetRotation, rotationSpeed);
        }
    }

    public static class Timing
    {
        public static bool CheckCooldown(float lastActionTime, float cooldownDuration)
        {
            return Time.time - lastActionTime >= cooldownDuration;
        }
    }
}