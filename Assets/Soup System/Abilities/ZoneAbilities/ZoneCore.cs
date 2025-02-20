using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class ZoneCore : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] ZoneArea zoneArea;

    AbilityStats stats;
    List<Infliction> inflictions;
    float persistenceTime;
    bool stuckToPlayer = true;

    public void Spawn(Vector2 spawnPoint, Vector2 dir, AbilityStats stats, List<Infliction> inflictions, bool onPlayer)
    {
        this.stats = stats;
        this.inflictions = inflictions;
        persistenceTime = 0;

        transform.position = spawnPoint;
        zoneArea.transform.localScale = new Vector3(stats.size, stats.size, stats.size);
        gameObject.SetActive(true);
        zoneArea.inflictions = inflictions;
        stuckToPlayer = onPlayer;
        print("Spawned zone with stats " + stats.duration.ToString());
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject))
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (stuckToPlayer)
        {
            transform.position = PlayerEntityManager.Singleton.GetPlayerPosition();
        }
    }

    private void FixedUpdate()
    {
        if (persistenceTime < stats.duration)
        {
            persistenceTime += Time.fixedDeltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
