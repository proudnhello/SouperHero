using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenNavigation : MonoBehaviour
{
    [SerializeField] GameObject exitPanel;

    // Goes back to Main Menu
    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    // Restarts the Game
    public void RestartGame()
    {
        SaveManager.Singleton.ResetGameState();
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    public void ShowExitConfirmation()
    {
        exitPanel.SetActive(true);
    }

    public void ConfirmedExit()
    {
        Application.Quit();  // Quits the game in a build
    }

    public void ReturnFromExit()
    {
        exitPanel.SetActive(false);
    }
}
