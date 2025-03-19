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
    [SerializeField] PixelPerfectCamera ppc;
    [SerializeField] Vector2 maxDistance;
    [SerializeField] Vector2 distanceDivider;
    [SerializeField] float smoothingTime = 1f;

    [Header("PID")]
    [SerializeField] float proportionalGain = 1f;
    [SerializeField] float derivativeGain = 5f;
    [SerializeField] float maxVelocity = 1f;
    float errorX, errorY, P, errorRateOfChange, D, errLastX, errLastY, velocityX, velocityY;
    [SerializeField] float eps = .2f;
    [SerializeField] float veps = .02f;
    [SerializeField] float deps = .2f;
    float dt, lastDt, maxDt;
    private Vector3 targetPos;
    Transform _player;


    // This method is called when the script instance is being loaded
    private void Start()
    {
        // Round the camera's local position to the nearest pixel for pixel-perfect rendering
        transform.localPosition = ppc.RoundToPixel(transform.localPosition);
        // Get the player's transform from the PlayerEntityManager singleton
        _player = PlayerEntityManager.Singleton.gameObject.transform;
        dt = lastDt = maxDt = Time.maximumDeltaTime;
        StartCoroutine(TargetFollow());
        PlayerEntityManager.Singleton.input.Player.ZoomOut.started += (ctx) => toggleZoomOut = !toggleZoomOut;
        StartCoroutine(HandleZoom());
    }

    bool toggleZoomOut = false;
    IEnumerator HandleZoom()
    {
        while (true)
        {
            ppc.assetsPPU = 32;
            yield return new WaitUntil(() => toggleZoomOut);
            if (CookingManager.Singleton.IsCooking() || PlayerEntityManager.Singleton.playerMovement.IsMoving()) 
            {
                toggleZoomOut = false;
            } else
            {
                ppc.assetsPPU = 20;
                yield return new WaitUntil(() => !toggleZoomOut ||
                    CookingManager.Singleton.IsCooking() ||
                    PlayerEntityManager.Singleton.playerMovement.IsMoving());
            }

        }
    }


    private void Update()
    {
        Debug.DrawRay(transform.position, targetPos - transform.position, Color.red);
    }

    // This method is called once per frame
    void CalculateTargetPos()
    {
        if (CookingManager.Singleton.IsCooking())
        {
            targetPos = CookingManager.Singleton.CurrentCampfire.transform.position + CookingManager.Singleton.CurrentCampfire.CameraOffset;
            targetPos = new Vector3(targetPos.x, targetPos.y, transform.localPosition.z);
        } else
        {
            // Round the player position first to ensure consistent base position
            Vector3 roundedPlayerPos = ppc.RoundToPixel(_player.position);

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Calculate the target distance based on the mouse position and player position, divided by the distance divider
            Vector3 targetDist = (mousePos - roundedPlayerPos) / distanceDivider;

            // Clamp the target position within the specified maximum distance and add the player's position
            targetPos = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x),
                                    Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y),
                                    transform.localPosition.z) + roundedPlayerPos;
        }      
    }

    private IEnumerator TargetFollow()
    {
        while (true)
        {
            errorX = errLastX = errorY = errLastY = velocityX = velocityY = 0;

            yield return new WaitUntil(() => !GameManager.isPaused);

            CalculateTargetPos();          

            if (Mathf.Abs(Vector3.Distance(targetPos, transform.localPosition)) > deps)
            {       
                do
                {
                    dt = Time.deltaTime <= 0 || Time.deltaTime >= maxDt ? lastDt : Time.deltaTime;
                    lastDt = dt;

                    errorX = targetPos.x - transform.localPosition.x;
                    P = proportionalGain * errorX;                 
                    errorRateOfChange = (errorX - errLastX) / dt;

                    errLastX = errorX;
                    D = derivativeGain * errorRateOfChange;
                    velocityX += P + D;
                    velocityX = Mathf.Clamp(velocityX, -maxVelocity, maxVelocity);

                    errorY = targetPos.y - transform.localPosition.y;
                    P = proportionalGain * errorY;
                    errorRateOfChange = (errorY - errLastY) / dt;
                    errLastY = errorY;
                    D = derivativeGain * errorRateOfChange;
                    velocityY += P + D;
                    velocityY = Mathf.Clamp(velocityY, -maxVelocity, maxVelocity);

                    transform.localPosition = ppc.RoundToPixel(new Vector3(transform.localPosition.x + velocityX, transform.localPosition.y + velocityY, transform.localPosition.z));
                    //Debug.Log($"{Time.time}: vel = ({velocityX},{velocityY}), error = ({errorX},{errorY}), P={P},D={D}, dis = {Mathf.Abs(Vector3.Distance(targetPos, transform.localPosition))}");

                    yield return new WaitForFixedUpdate();
                    CalculateTargetPos();
                } while (!GameManager.isPaused && (Mathf.Abs(errorX) > eps || Mathf.Abs(errorY) > eps || Mathf.Abs(velocityX) > veps || Mathf.Abs(velocityY) > veps 
                || Mathf.Abs(Vector3.Distance(targetPos, transform.localPosition)) > deps));
            }
            yield return new WaitForFixedUpdate();
        }     
    }
}
