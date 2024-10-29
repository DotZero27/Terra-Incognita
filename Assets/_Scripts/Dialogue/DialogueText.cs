using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    [Header("Speaker Information")]
    public string speakerName;

    [Header("Dialogue Content")]
    [TextArea(5, 10)]
    public string[] paragraphs;

    private void OnValidate()
    {
        // Ensure we don't have any null or empty paragraphs
        if (paragraphs != null)
        {
            for (int i = 0; i < paragraphs.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paragraphs[i]))
                {
                    Debug.LogWarning($"Empty paragraph found at index {i} in {name}");
                }
            }
        }
    }
}