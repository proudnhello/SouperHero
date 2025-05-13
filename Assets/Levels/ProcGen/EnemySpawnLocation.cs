using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawnLocation : MonoBehaviour
{
    internal int index = -1;
    public List<PossibleEnemy> possibleEnemies;

    [Serializable]
    public class PossibleEnemy
    {
        public GameObject enemyChoice;
        public int weight;
    }

    PossibleEnemy GetEnemy()
    {
        int totalWeight = possibleEnemies.Sum(x => x.weight);
        int rand = UnityEngine.Random.Range(0, totalWeight);
        PossibleEnemy selectedEnemy = null;
        foreach (var enemy in possibleEnemies)
        {
            if (rand < enemy.weight)
            {
                selectedEnemy = enemy;
                break;
            }
            rand -= enemy.weight;
        }
        return selectedEnemy;
    }

    public void SpawnEnemy()
    {
        PossibleEnemy enemyToSpawn = GetEnemy();
        if (enemyToSpawn.enemyChoice == null) return;
        index = RunStateManager.Singleton.GetNewEnemyIndex();
        if (RunStateManager.Singleton.IsEnemyAlive(index))
        {
            GameObject enemy = Instantiate(enemyToSpawn.enemyChoice, transform.position, Quaternion.identity, transform);
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
