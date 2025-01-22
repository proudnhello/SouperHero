using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    void Start()
    {
        PlayerManager.instance.SetHealth(PlayerManager.instance.GetMaxHealth());
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7)    // 7 is the layer for enemy
        {
            PlayerManager.instance.TakeDamage(10);
        }
    }
}
