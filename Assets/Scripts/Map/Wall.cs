using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] GameObject door;
    public void OpenDoor() {
        door.SetActive(false);
    }
}
