using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueContoller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 1f;

    private Queue<string> paragraphs = new Queue<string>();
    private string lastDialogue;
    private bool conversationEnded;
    private bool isTyping;
    private bool hasCompletedAllDialogues;
    private bool isPlayingLastDialogue;
    private bool hasShownEndingDialogue;

    private string p;
    private Coroutine typeDialogueCoroutine;
    private const float MAX_TYPE_TIME = 0.1f;

    private void Awake()
    {
        // Start disabled - will be enabled when NPC is initialized
        gameObject.SetActive(false);
    }

    public void DisplayNextParagraph(DialogueText dialogueText)
    {
        if (dialogueText == null)
        {
            Debug.LogError("DialogueText is null");
            return;
        }

        // Ensure the dialogue UI is enabled when displaying text
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // If we've completed all dialogues and shown the ending dialogue, start repeating last dialogue
        if (hasCompletedAllDialogues && hasShownEndingDialogue)
        {
            if (isPlayingLastDialogue && !isTyping)
            {
                EndConversation();
                isPlayingLastDialogue = false;
                return;
            }
            PlayLastDialogue();
            return;
        }

        // If we've just completed all dialogues but haven't shown ending dialogue yet
        if (hasCompletedAllDialogues && !hasShownEndingDialogue)
        {
            EndConversation();
            hasShownEndingDialogue = true;
            return;
        }

        if (paragraphs.Count == 0)
        {
            if (!conversationEnded)
            {
                StartConversation(dialogueText);
            }
            else if (!isTyping)
            {
                EndConversation();
                return;
            }
        }

        if (!isTyping && paragraphs.Count > 0)
        {
            p = paragraphs.Dequeue();
            lastDialogue = p;
            typeDialogueCoroutine = StartCoroutine(TypeDialogueText(p));
        }
        else if (isTyping)
        {
            FinishParagraphEarly();
        }

        if (paragraphs.Count == 0)
        {
            conversationEnded = true;
            hasCompletedAllDialogues = true;
        }
    }

    private void PlayLastDialogue()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        isPlayingLastDialogue = true;
        
        if (!isTyping)
        {
            typeDialogueCoroutine = StartCoroutine(TypeDialogueText(lastDialogue));
        }
        else
        {
            FinishParagraphEarly();
        }
    }

    private void StartConversation(DialogueText dialogueText)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        NPCNameText.text = dialogueText.speakerName;

        foreach (string paragraph in dialogueText.paragraphs)
        {
            if (!string.IsNullOrEmpty(paragraph))
            {
                paragraphs.Enqueue(paragraph);
            }
        }
    }

    private void EndConversation()
    {
        paragraphs.Clear();
        conversationEnded = true;
        gameObject.SetActive(false);
    }

    public void ForceEndConversation()
    {
        if (isTyping && typeDialogueCoroutine != null)
        {
            StopCoroutine(typeDialogueCoroutine);
            isTyping = false;
        }
        EndConversation();
    }

    private IEnumerator TypeDialogueText(string text)
    {
        isTyping = true;

        int maxVisibleChars = 0;
        NPCDialogueText.text = text;
        NPCDialogueText.maxVisibleCharacters = maxVisibleChars;

        foreach (char c in text.ToCharArray())
        {
            maxVisibleChars++;
            NPCDialogueText.maxVisibleCharacters = maxVisibleChars;
            yield return new WaitForSeconds(MAX_TYPE_TIME / typeSpeed);
        }

        isTyping = false;
    }

    private void FinishParagraphEarly()
    {
        if (typeDialogueCoroutine != null)
        {
            StopCoroutine(typeDialogueCoroutine);
        }
        
        NPCDialogueText.text = p;
        NPCDialogueText.maxVisibleCharacters = p.Length;
        isTyping = false;
    }

    public void Reset()
    {
        hasCompletedAllDialogues = false;
        hasShownEndingDialogue = false;
        conversationEnded = false;
        isPlayingLastDialogue = false;
        paragraphs.Clear();
        lastDialogue = "";
        
        if (isTyping && typeDialogueCoroutine != null)
        {
            StopCoroutine(typeDialogueCoroutine);
            isTyping = false;
        }

        gameObject.SetActive(false);
    }
}