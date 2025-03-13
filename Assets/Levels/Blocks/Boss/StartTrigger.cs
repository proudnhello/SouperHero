using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    [SerializeField] BossRoom room;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        room.BeginFight();
    }
}
