using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RunStateManager : MonoBehaviour
{
    public static RunStateManager Singleton { get; private set; }
    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    [Serializable]
    public class RunStateData
    {
        public List<byte> enemyStates;
        public List<byte> destroyableStates;
        public int seed;
        public Vector3 playerPos;

        public RunStateData(bool newData = false)
        {
            if (newData)
            {
                enemyStates = new();
                destroyableStates = new();
                playerPos = Vector3.zero;
            }
        }

        public bool GetEnemyState(int index)
        {
            if (index >= enemyStates.Count)
            {
                return false;
            }
            else if (enemyStates[index] == 1) return true; // 1 == alive
            else return false; // 0 == dead
        }

        public bool GetDestroyableState(int index)
        {
            if (index >= destroyableStates.Count)
            {
                return false;
            }
            else if (destroyableStates[index] == 1) return true; // 1 == alive
            else return false; // 0 == dead
        }
    }

    int enemySpawnIndex, destroyableSpawnIndex;
    RunStateData runData;

    public void InitializeGameState(int initialSeed = -1)
    {
        runData = SaveManager.Singleton.LoadRunState();
        if (runData == null)
        {
            runData = new(true);
            runData.seed = initialSeed < 0 ? UnityEngine.Random.Range(0, int.MaxValue) : initialSeed;
        }

        enemySpawnIndex = 0;
        destroyableSpawnIndex = 0;

        UnityEngine.Random.InitState(runData.seed);
        Debug.Log("SEED: " + runData.seed);
    }

    public void InitialPlacePlayer(PlayerSpawnLocation startSpawn)
    {
        if (runData.playerPos == Vector3.zero)
        {
            startSpawn.SetPlayerPosition();
        }
        else
        {
            PlayerEntityManager.Singleton.transform.position = runData.playerPos;
        }
    }

    public void SaveRunState()
    {
        runData.playerPos = PlayerEntityManager.Singleton.transform.position;
        SaveManager.Singleton.SaveRunState(runData);
    }

    public int GetNewEnemyIndex()
    {
        runData.enemyStates.Add(1);
        enemySpawnIndex++;
        return enemySpawnIndex - 1;
    }

    public bool IsEnemyAlive(int index)
    {
        return runData.GetEnemyState(index);
    }

    public void TrackEnemyDeath(int index)
    {
        runData.enemyStates[index] = 0;
    }

    public int GetNewDestroyableIndex()
    {
        runData.destroyableStates.Add(1);
        destroyableSpawnIndex++;
        return destroyableSpawnIndex - 1;
    }

    public bool HasDestroyableBeenBroken(int index)
    {
        return runData.GetDestroyableState(index);
    }

    public void TrackBrokenDestroyable(int index)
    {
        runData.destroyableStates[index] = 0;
    }
}
