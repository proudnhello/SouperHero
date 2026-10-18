using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

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

        foreach (var sentenceKey in dialogue.sentenceKeys)
        {
            if (dialogue.currentStage == sentenceKey.stage) sentenceKeys.Enqueue(sentenceKey.sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentenceKeys.Count == 0)
        {
            EndDialogue();
            return;
        }

        string rawSentence = LocalizationManager.GetLocalizedDialogue(sentenceKeys.Dequeue());
        string currSentence = "";
        for (int i = 0; i < rawSentence.Length; i++)
        {
            char c = rawSentence[i];
            currSentence += c;
            if (c == '[')
            {
                i++;
                c = rawSentence[i];
                string key = "";
                do
                {
                    key += c;
                    i++;
                    c = rawSentence[i];
                } while (c != ']');
                Debug.Log(key);
                currSentence += PlayerKeybinds.Singleton.GetBindingByActionName(key);
                currSentence += c;
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currSentence));
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
        isDialogueActive = false;
        animator.SetBool("IsOpen", false);
    }

}
