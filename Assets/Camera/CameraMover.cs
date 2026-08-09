using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Cinemachine;
using UnityEngine.InputSystem;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraMover : MonoBehaviour
{
    // Singleton instance, probably a bad idea, but whatever
    public static CameraMover Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
    }

    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] CinemachineVirtualCamera _cam;

    [SerializeField] float UNITS_PER_PIXEL;
    [SerializeField] Vector2 maxDistance;
    [SerializeField] Vector2 distanceDivider;
    Transform _player;

    [Header("Zoom Anim")]
    [SerializeField] AnimationCurve ZoomAnimationCurve;
    [SerializeField] float ZoomAnimationTime;
    float ZoomedOutSize;
    float ZoomedInSize;


    // This method is called when the script instance is being loaded
    private void Start()
    {
        // Get the player's transform from the PlayerEntityManager singleton
        _player = PlayerEntityManager.Singleton.gameObject.transform;
        transform.position = new Vector3(_player.position.x, _player.position.y, transform.position.z);
        Camera.main.transform.position = transform.position;
        ZoomedOutSize = _cam.m_Lens.OrthographicSize = 0.5f * UNITS_PER_PIXEL * Screen.height;
        ZoomedInSize = ZoomedOutSize / 2;
        CookingScreen.EnterCookingScreen += ZoomIn;
        CookingScreen.ExitCookingScreen += ZoomOut;
    }

    private void OnDisable()
    {
        CookingScreen.EnterCookingScreen -= ZoomIn;
        CookingScreen.ExitCookingScreen -= ZoomOut;
    }

    void ZoomIn()
    {
        if (IZoomAnim != null) StopCoroutine(IZoomAnim);
        StartCoroutine(IZoomAnim = ZoomAnim(true));
    }
    void ZoomOut()
    {
        if (IZoomAnim != null) StopCoroutine(IZoomAnim);
        StartCoroutine(IZoomAnim = ZoomAnim(false));
    }

    float zoomTimeProgressed;
    IEnumerator IZoomAnim;
    bool zoomInProgress;
    Vector3 GoalPos;
    Vector3 ShakeOffset = Vector3.zero;
    IEnumerator ZoomAnim(bool zoomIn)
    {
        zoomInProgress = true;

        Vector3 ReturnPos;
        if (zoomIn)
        {
            ReturnPos = transform.position;
            Vector3 t = CookingScreen.Singleton.CurrentCampfire.transform.position + CookingScreen.Singleton.CurrentCampfire.CameraOffset;
            GoalPos = new Vector3(t.x, t.y, transform.position.z);
        }
        else
        {
            ReturnPos = TargetPos;
        }

        while (zoomTimeProgressed >= 0 && zoomTimeProgressed <= ZoomAnimationTime)
        {
            var percentCompleted = Mathf.Clamp01(zoomTimeProgressed / ZoomAnimationTime);
            var scaledPercentaged = ZoomAnimationCurve.Evaluate(percentCompleted);

            var newOrthoSize = Mathf.Lerp(ZoomedOutSize, ZoomedInSize, scaledPercentaged);

            if (!zoomIn) ReturnPos = TargetPos;
            var newPosition = Vector3.Lerp(ReturnPos, GoalPos, scaledPercentaged);

            _cam.m_Lens.OrthographicSize = newOrthoSize;
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            yield return null;

            zoomTimeProgressed = zoomIn ? zoomTimeProgressed + Time.deltaTime : zoomTimeProgressed - Time.deltaTime;
        }

        zoomTimeProgressed = zoomIn ? ZoomAnimationTime : 0;
        _cam.m_Lens.OrthographicSize = zoomIn ? ZoomedInSize : ZoomedOutSize;
        transform.position = zoomIn ? GoalPos : ReturnPos;
        IZoomAnim = null;
        if (!zoomIn) zoomInProgress = false;
    }

    private void Update()
    {
        CalculateTargetPos();
        if (!zoomInProgress)
        {
            transform.position = TargetPos + ShakeOffset;
        }
    }

    public void ScreenShake(float intensity, float shakeLength, float totalLength)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity, shakeLength, totalLength));
    }

    private IEnumerator ScreenShakeCoroutine(float intensity, float shakeLength, float totalLength)
    {
        float timer = 0f;
        while (timer < totalLength)
        {
            float offsetX = UnityEngine.Random.Range(-intensity, intensity);
            float offsetY = UnityEngine.Random.Range(-intensity, intensity);
            ShakeOffset = new Vector3(offsetX, offsetY, 0);
            yield return new WaitForSeconds(shakeLength);
            timer += shakeLength;
        }

        ShakeOffset = Vector3.zero;
    }

    Vector3 TargetPos;
    void CalculateTargetPos()
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Calculate the target distance based on the mouse position and player position, divided by the distance divider
        Vector3 targetDist = (mousePos - _player.position) / distanceDivider;

        // Clamp the target position within the specified maximum distance and add the player's position
        TargetPos = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x) + _player.position.x,
                                Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y) + _player.position.y, transform.position.z);
    }
}
