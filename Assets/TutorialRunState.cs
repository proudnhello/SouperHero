using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class TutorialRunState : MonoBehaviour
{
    // This script is just used to set up the tutorial so we don't get nasty errors, 
    // everything subject to change since this is just for tutorial
    [SerializeField] private PlayerSpawnLocation playerSpawnLocation;

    [SerializeField] private NavMeshSurface _navMeshSurface;

    [SerializeField] private GameObject helperScripts;

    
    void Start()
    {
        helperScripts.GetComponent<RoomGenerator>().enabled = false; // Disable room generation for the tutorial
        RunStateManager.Singleton.InitializeGameState(-1);  // -1 = no seed, use random

        RunStateManager.Singleton.SaveRunState();

        // Set player spawn location so the Run State Manager doesn't complain about it
        RunStateManager.Singleton.InitialPlacePlayer(playerSpawnLocation);

        // bake the navmesh so the enemy can pathfind
        _navMeshSurface.BuildNavMeshAsync();
    }
}
