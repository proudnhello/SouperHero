using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreenNavigation : MonoBehaviour
{
    [SerializeField] MainMenuSceneTransition trans;
    [SerializeField] GraphicRaycaster graphicRaycaster;

    // Goes back to Main Menu
    int queueScene = -1;
    public void MainMenu()
    {
        if (isEnding) return;
        queueScene = 0;
        StartCoroutine(EndTutorialTransition());
    }
    // Restarts the Game
    public void RestartGame()
    {
        if (isEnding) return;

        SaveManager.Singleton.ResetGameState();
        queueScene = 1;
        StartCoroutine(EndTutorialTransition());
    }


    bool isEnding = false;
    IEnumerator EndTutorialTransition()
    {
        graphicRaycaster.enabled = false;
        isEnding = true;
        trans.FadeToBlack();
        yield return new WaitUntil(() => !trans.inProgress);
        SceneManager.LoadScene(queueScene);
    }
}
