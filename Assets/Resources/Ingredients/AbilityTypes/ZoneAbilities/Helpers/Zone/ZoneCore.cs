using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class ZoneCore : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] ZoneArea zoneArea;
    PlayerCenteredZone playerCenteredZone;

    AbilityStats stats;
    List<Infliction> inflictions;
    float persistenceTime;
    bool stuckToPlayer = true;

    public void Spawn(Vector2 spawnPoint, Vector2 dir, AbilityStats passedStats, List<Infliction> inflictions, bool onPlayer, PlayerCenteredZone ability)
    {
        this.stats = passedStats;
        this.inflictions = inflictions;
        persistenceTime = 0;

        transform.position = spawnPoint;
        Debug.Log($"Zone Size Stats in Spawn Zone {passedStats.size}");
        zoneArea.transform.localScale = new Vector3(passedStats.size, passedStats.size, passedStats.size);
        gameObject.SetActive(true);
        zoneArea.inflictions = inflictions;
        stuckToPlayer = onPlayer;
        playerCenteredZone = ability;
        if(!stuckToPlayer)
        {
            rb.velocity = dir * stats.speed;
        }
        print("Spawned zone with stats " + stats.duration.ToString());
    }

    public ZoneArea GetZoneArea()
    {
        return zoneArea;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject))
        {
            gameObject.SetActive(false);
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
            if(stuckToPlayer && playerCenteredZone)
            {
                playerCenteredZone.Deactivate();
            }
            gameObject.SetActive(false);
        }
    }
}
