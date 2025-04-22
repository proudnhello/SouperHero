using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    private Queue<string> sentences;
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
        sentences = new Queue<string>();
        nameText.text = "";
        dialogueText.text = "";
    }

    public void StartDialogue(Dialogue dialogue)
    {
        nameText.text = dialogue.name;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0){
            EndDialogue();
            return;
        }

        // ----- ONCE WORKING SWAP THIS OUT FOR A LOCALIZATION CALL ------
        string sentence = sentences.Dequeue(); 
        dialogueText.text = sentence;
        Debug.Log(sentence);
        // -------------------------------------------------------------
    }

    void EndDialogue()
    {
        Debug.Log("End of conversation.");
    }

}
