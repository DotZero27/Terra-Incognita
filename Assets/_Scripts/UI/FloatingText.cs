using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float DestroyTime = 0.4f;
    public Vector3 offset = new Vector3(0, 2f, 0); // Adjust for top-down, no z-offset
    public Vector3 RandomizeIntensity = new Vector3(0.5f, 0.5f, 0); // Randomize only in x and y

    void Start()
    {
        Destroy(gameObject, DestroyTime);

        // Position the text above the object and add random offset
        transform.position += offset;
        transform.localPosition += new Vector3(
            Random.Range(-RandomizeIntensity.x, RandomizeIntensity.x),
            Random.Range(-RandomizeIntensity.y, RandomizeIntensity.y),
            0 // Keep z position static for a top-down view
        );
    }
}
