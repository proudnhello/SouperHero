using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    GameObject player;
    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] float displacementMult = 0.15f;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerManager.instance.player;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera aim ajustment based on this tutorial: https://www.youtube.com/watch?v=hXR103eZpHU
        if (player == null)
        {
            return;
        }
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cameraDisplacement = (mousePosition - player.transform.position) * displacementMult;

        Vector3 targetPosition = player.transform.position + cameraDisplacement;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
    }
}
