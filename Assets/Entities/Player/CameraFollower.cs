using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollower : MonoBehaviour
{
    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] float maxDistance;
    [SerializeField] float distanceMult;

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
        Vector3 targetDist = (mousePos - _player.position) / distanceMult;

        Vector3 targetPos = Vector3.ClampMagnitude(targetDist, maxDistance) + _player.position;
        targetPos.z = 0;

        transform.position = targetPos;
    }
}
