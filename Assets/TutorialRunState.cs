using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRunState : MonoBehaviour
{
    [SerializeField] private PlayerSpawnLocation playerSpawnLocation;
    // Start is called before the first frame update
    void Start()
    {
        RunStateManager.Singleton.InitializeGameState(-1);  // -1 = no seed, use random

        RunStateManager.Singleton.SaveRunState();

        // Set player spawn location so the Run State Manager doesn't complain about it
        RunStateManager.Singleton.InitialPlacePlayer(playerSpawnLocation);
    }
}
