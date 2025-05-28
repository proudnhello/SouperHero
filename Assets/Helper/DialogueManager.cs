using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    private Queue<string> sentenceKeys;

    public Animator animator;

    private bool isDialogueActive = false;
    
    public static DialogueManager Singleton { get; private set; }
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sentenceKeys = new Queue<string>();
        nameText.text = "";
        dialogueText.text = "";
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        animator.SetBool("IsOpen", true);
        nameText.text = LocalizationManager.GetLocalizedDialogue(dialogue.nameKey);
        sentenceKeys.Clear();

        foreach (string sentenceKey in dialogue.sentenceKeys)
        {
            sentenceKeys.Enqueue(sentenceKey);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentenceKeys.Count == 0){
            isDialogueActive = false;
            EndDialogue();
            return;
        }

        string sentence = LocalizationManager.GetLocalizedDialogue(sentenceKeys.Dequeue()); 
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    public void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
    }

}
