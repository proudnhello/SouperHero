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

    private void Start()
    {
        transform.localPosition = ppc.RoundToPixel(transform.localPosition);
        _player = PlayerEntityManager.Singleton.gameObject.transform;

    }

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetDist = (mousePos - _player.position) / distanceDivider;

        targetPos = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x),
                                        Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y), 0)
                                        + _player.position;
    }

    void LateUpdate()
    {      
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothingTime);
        transform.localPosition = ppc.RoundToPixel(new Vector3(newPos.x, newPos.y, transform.position.z));
    }
}
