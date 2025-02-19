using UnityEngine;

public class EndGame : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.instance.WinScreen();
    }
}
