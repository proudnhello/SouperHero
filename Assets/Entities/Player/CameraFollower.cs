using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    GameObject player;
    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] float displacementMult = 0.15f;
    private bool _useMouse;
    private Vector2 _previousMousePosition;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerEntityManager.Singleton.gameObject;
        _previousMousePosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera aim ajustment based on this tutorial: https://www.youtube.com/watch?v=hXR103eZpHU
        if (player == null)
        {
            return;
        }

        // This code is duplicated in PlayerControl.cs. We should consolidate the two into a singleton.

        // Check if the mouse is active
        if (((Vector2) Input.mousePosition - _previousMousePosition).sqrMagnitude > 0.01f)
        {
            _useMouse = true;
        } else
        {
            _useMouse = false;
        }

        Vector2 keyDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow))
            keyDirection += Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow))
            keyDirection += Vector2.down;
        if (Input.GetKey(KeyCode.LeftArrow))
            keyDirection += Vector2.left;
        if (Input.GetKey(KeyCode.RightArrow))
            keyDirection += Vector2.right;

        keyDirection = keyDirection.normalized * 5;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cameraDisplacement = _useMouse || keyDirection == Vector2.zero ? (mousePosition - player.transform.position) * displacementMult : keyDirection * displacementMult;

        Vector3 targetPosition = player.transform.position + cameraDisplacement;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        _previousMousePosition = Input.mousePosition;
    }
}
