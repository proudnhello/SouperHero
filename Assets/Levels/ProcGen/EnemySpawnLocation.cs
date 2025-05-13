using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class EnemySpawnLocation : MonoBehaviour
{
    internal int index = -1;
    public List<GameObject> possibleEnemies;

    public void SpawnEnemy()
    {
        GameObject enemyToSpawn = possibleEnemies[UnityEngine.Random.Range(0, possibleEnemies.Count)];
        index = RunStateManager.Singleton.GetNewEnemyIndex();
        if (RunStateManager.Singleton.IsEnemyAlive(index))
        {
            GameObject enemy = Instantiate(enemyToSpawn, transform.position, Quaternion.identity, transform);
            enemy.GetComponent<EnemyBaseClass>().SetIndex(index);
        }
    }

    private void OnDrawGizmos()
    {
        // Set the color with custom alpha.
        Gizmos.color = new Color(1f, 0f, 0f, .5f); // Red with custom alpha
        Gizmos.DrawSphere(transform.position, .5f);
    }
}
