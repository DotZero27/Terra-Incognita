using UnityEngine;

public class Lemon : NPC, ITalkable
{
    [SerializeField] private DialogueText dialogueText;

    public override void Interact()
    {
        if (dialogueController == null)
        {
            Debug.LogError($"DialogueController is null on {gameObject.name}");
            return;
        }

        isInteracting = true;
        Talk(dialogueText);
    }

    public void Talk(DialogueText dialogueText)
    {
        if (dialogueController != null && dialogueText != null)
        {
            dialogueController.DisplayNextParagraph(dialogueText);
        }
        else
        {
            Debug.LogError($"Missing references in {gameObject.name}. DialogueController: {dialogueController != null}, DialogueText: {dialogueText != null}");
        }
    }

    public void ResetDialogue()
    {
        if (dialogueController != null)
        {
            dialogueController.Reset();
        }
    }
}