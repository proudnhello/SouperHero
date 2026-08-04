using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    [SerializeField] BossAI boss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player") {
            return; 
        }
        boss.TriggerBossFight();
        gameObject.SetActive(false);
    }
}
