using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraMover : MonoBehaviour
{

    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] Camera _cam;
    [SerializeField] float UNITS_PER_PIXEL;
    [SerializeField] Vector2 maxDistance;
    [SerializeField] Vector2 distanceDivider;
    Transform _player;


    // This method is called when the script instance is being loaded
    private void Start()
    {
        // Get the player's transform from the PlayerEntityManager singleton
        _player = PlayerEntityManager.Singleton.gameObject.transform;
        transform.position = new Vector3(_player.position.x, _player.position.y, transform.position.z);
        _cam.transform.position = transform.position;
        //StartCoroutine(TargetFollow());
        PlayerEntityManager.Singleton.input.Player.ZoomOut.started += (ctx) => toggleZoomOut = !toggleZoomOut;
        _cam.orthographicSize = 0.5f * UNITS_PER_PIXEL * Screen.height;
        StartCoroutine(HandleZoom());
    }

    bool toggleZoomOut = false;
    IEnumerator HandleZoom()
    {
        while (true)
        {
            _cam.orthographicSize = 0.5f * UNITS_PER_PIXEL * Screen.height;

            yield return new WaitUntil(() => toggleZoomOut);
            if (CookingManager.Singleton.IsCooking() || PlayerEntityManager.Singleton.playerMovement.IsMoving()) 
            {
                toggleZoomOut = false;
            } else
            {
                _cam.orthographicSize = 0.25f * UNITS_PER_PIXEL * Screen.height;
                yield return new WaitUntil(() => !toggleZoomOut ||
                    CookingManager.Singleton.IsCooking() ||
                    PlayerEntityManager.Singleton.playerMovement.IsMoving());
            }

        }
    }

    private void Update()
    {
        CalculateTargetPos();
    }

    // This method is called once per frame
    void CalculateTargetPos()
    {
        if (CookingManager.Singleton.IsCooking())
        {
            Vector3 newPos = CookingManager.Singleton.CurrentCampfire.transform.position + CookingManager.Singleton.CurrentCampfire.CameraOffset;
            transform.position = new Vector3(newPos.x, newPos.y, -20);
        } else
        {

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Calculate the target distance based on the mouse position and player position, divided by the distance divider
            Vector3 targetDist = (mousePos - _player.position) / distanceDivider;

            // Clamp the target position within the specified maximum distance and add the player's position
            transform.position = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x) + _player.position.x,
                                    Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y) + _player.position.y, transform.position.z);
        }      
    }
}

