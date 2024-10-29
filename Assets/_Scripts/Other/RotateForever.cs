using UnityEngine;

public class RotateForever : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = new Vector3(1f, 1f, 0.5f);
    public float rotationSpeed = 22f;

    private void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}