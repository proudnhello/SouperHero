using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraMover : MonoBehaviour
{
    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] PixelPerfectCamera ppc;
    [SerializeField] Vector2 maxDistance;
    [SerializeField] Vector2 distanceDivider;

    [SerializeField] float smoothingTime = 1f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPos;
    Transform _player;

    // This method is called when the script instance is being loaded
    private void Start()
    {
        // Round the camera's local position to the nearest pixel for pixel-perfect rendering
        transform.localPosition = ppc.RoundToPixel(transform.localPosition);
        // Get the player's transform from the PlayerEntityManager singleton
        _player = PlayerEntityManager.Singleton.gameObject.transform;
    }

    // This method is called once per frame
    private void Update()
    {
        // Round the player position first to ensure consistent base position
        Vector3 roundedPlayerPos = ppc.RoundToPixel(_player.position);

        // Test if the jitter is player movement or camera
        //transform.position = ppc.RoundToPixel(new Vector3(roundedPlayerPos.x, roundedPlayerPos.y, transform.position.z));

        // Get the current mouse position in world coordinates
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Calculate the target distance based on the mouse position and player position, divided by the distance divider
        Vector3 targetDist = (mousePos - roundedPlayerPos) / distanceDivider;

        // Clamp the target position within the specified maximum distance and add the player's position
        targetPos = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x),
                                Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y),
                                0) + roundedPlayerPos;

        //// Test if the jitter is targetPos
        //transform.position = ppc.RoundToPixel(new Vector3(targetPos.x, targetPos.y, transform.position.z));
    }

    // This method is called after all Update methods have been called
    void LateUpdate()
    {
        // Smoothly transition the camera's position towards the target position
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothingTime);
        // Set the camera's local position, rounding it to the nearest pixel
        transform.position = ppc.RoundToPixel(new Vector3(newPos.x, newPos.y, transform.position.z));
    }
}
