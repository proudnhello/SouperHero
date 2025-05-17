using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorial : MonoBehaviour
{
    [SerializeField] private GameObject helperScripts;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // enable room generation
            // helperScripts.GetComponent<RoomGenerator>().enabled = true; // Enable room generation for the tutorial
            RoomGenerator roomGenerator = helperScripts.GetComponent<RoomGenerator>();
            if (roomGenerator != null)
            {
                roomGenerator.enabled = true; // Disable room generation for the tutorial
            }

            Destroy(helperScripts);

            GameManager.Singleton.NewGame();
            // SceneManager.LoadScene(0);
        }
    }
}
