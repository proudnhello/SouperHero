using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapFollow : MonoBehaviour
{
    Transform playerRef;

    void Awake()
    {
        playerRef = GameObject.Find("Player").transform;
    }

    void LateUpdate()
    {
        Vector3 newPos = playerRef.position;

        newPos.z = transform.position.z;

        transform.position = newPos;
    }
}
