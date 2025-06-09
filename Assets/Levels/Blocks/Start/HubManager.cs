using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.Singleton._MusicHandler.ChangeState(MusicHandler.MusicState.HUB);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.Singleton._MusicHandler.ChangeState(MusicHandler.MusicState.EXPLORATION);
        }
    }
}
