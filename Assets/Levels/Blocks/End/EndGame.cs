using UnityEngine;

public class EndGame : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            MetricsManager.Singleton.RecordNumWins();
            MetricsManager.Singleton.RecordNumDeaths();
            MetricsManager.Singleton.SaveToMetricsToSO();
            GameManager.instance.WinScreen();
        }
    }
}
