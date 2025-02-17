using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollower : MonoBehaviour
{
    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] Vector2 maxDistance;
    [SerializeField] Vector2 distanceDivider;

    CinemachineVirtualCamera vcam;
    Transform _player;

    void Start()
    {
        vcam = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();
        vcam.Follow = transform;
        _player = PlayerEntityManager.Singleton.gameObject.transform;
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetDist = (mousePos - _player.position) / distanceDivider;

        Vector3 targetPos = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x),
                                        Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y), 0)
                                        + _player.position;

        transform.position = targetPos;
    }
}
