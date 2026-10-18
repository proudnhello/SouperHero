using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorial : MonoBehaviour
{
    [SerializeField] GameObject Door;
    [SerializeField] DialogueTrigger dialogueCookChange;
    [SerializeField] MainMenuSceneTransition trans;

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
        if (isEnding) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(EndTutorialTransition());
        }
    }

    bool isEnding = false;
    IEnumerator EndTutorialTransition()
    {
        isEnding = true;
        trans.FadeToBlack();
        GameManager.Singleton.EndRun(true);

        yield return new WaitUntil(() => !trans.inProgress);
        GameManager.Singleton.GameRunning = false;
        SceneManager.LoadScene(0);
    }
}
