using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorial : MonoBehaviour
{
    [SerializeField] GameObject Door;
    [SerializeField] DialogueTrigger dialogueCookChange;

    private void Start()
    {
        Door.SetActive(true);
        CookingScreen.CookSoup += OnCook;
    }

    private void OnDisable()
    {
        CookingScreen.CookSoup -= OnCook;
    }

    void OnCook()
    {
        Door.SetActive(false);
        dialogueCookChange.dialogue.currentStage = 1;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Singleton.EndRun(true);
        }
    }
}
