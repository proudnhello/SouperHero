using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSetup : MonoBehaviour
{
    // This script is just used to set up the tutorial so we don't get nasty errors, 
    // everything subject to change since this is just for tutorial
    [SerializeField] private PlayerSpawnLocation playerSpawnLocation;

    [SerializeField] private NavMeshSurface _navMeshSurface;
    [SerializeField] float camZoomOutStartTime = 1.5f;


    void Start()
    {
        // Set player spawn location so the Run State Manager doesn't complain about it
        playerSpawnLocation.SetPlayerPosition();

        // bake the navmesh so the enemy can pathfind
        _navMeshSurface.BuildNavMeshAsync();

        GameManager.Singleton.StartRun(true);
        GameManager.Singleton.GameRunning = true;

        // DEBUG.. if not coming from load scene then just zoom out camera
        if (!SceneManager.GetSceneByBuildIndex(1).IsValid()) CameraMover.Singleton.StartCoroutine(CameraMover.Singleton.ZoomAnimIntro(0, camZoomOutStartTime));
    }
}
