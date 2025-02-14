using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Unity.VisualScripting.Member;
using UnityEngine.SceneManagement;

public class PlayerHealth
{
    PlayerEntityManager Player;
    public static event Action HealthChange;

    internal bool invincible = false;

    public PlayerHealth(PlayerEntityManager player)
    {
        Player = player;
    }

    public void TakeDamage()
    {   
        // check if the player is still alive if so, go to game over screen
        if (PlayerEntityManager.Singleton.IsDead())
        {
            GameManager.instance.DeathScreen();
            return;
        }

        // make the player invincible for a short time
        invincible = true;
        // play the damage animation
        Player.StartCoroutine(Player.renderer.TakeDamageAnimation());
    }

    public void ResetInvincibility()
    {
        invincible = false;
    }

    public void ChangeHealth()
    {
        HealthChange?.Invoke();
    }
  
}
