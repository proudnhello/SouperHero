using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Unity.VisualScripting.Member;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private float damageTime = 1.0f;   // time the player flashes red when taking damage
    private bool invincible = false;

    public static event Action HealthChange;
    void Start()
    {
        PlayerManager.instance.SetHealth(PlayerManager.instance.GetMaxHealth());
    }

    void Update()
    {
        
    }

    public bool IsInvincible()
    {
        return invincible;
    }

    public void TakeDamage(int amount, GameObject source)
    {
        // make the player invincible for a short time
        invincible = true;

        // take damage based on the enemy's collision damage
        PlayerManager.instance.TakeDamage(amount);

        // knock back the player
        KnockBack(source);

        // check if the player is still alive if so, go to game over screen
        if (PlayerManager.instance.IsDead()){
            GameManager.instance.DeathScreen();
        }

        // play the damage animation
        StartCoroutine(TakeDamageAnimation());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if the collision is with an enemy and if the player is not invincible
        if (collision.gameObject.tag == "Enemy" && !invincible && !collision.gameObject.GetComponent<EnemyBaseClass>().getSoupable())    
        {
            TakeDamage(collision.gameObject.GetComponent<EnemyBaseClass>().playerCollisionDamage, collision.gameObject);
        }
    }

    private void KnockBack(GameObject collider)
    {
        // not functioning properly, need to fix
        // also not super necessary at the moment
        GameObject player = PlayerManager.instance.player;
        Vector3 direction = (player.transform.position - collider.transform.position).normalized;
        player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        player.GetComponent<Rigidbody2D>().AddForce(direction * 3, ForceMode2D.Impulse);
    }

    public IEnumerator TakeDamageAnimation(){

        float maxFlashCycles = ((damageTime / 0.3f));
        int flashCycles = 0;
        Color playerColor = PlayerManager.instance.player.GetComponent<SpriteRenderer>().color;
        HealthChange?.Invoke();

        while (maxFlashCycles > flashCycles)
        {
            PlayerManager.instance.player.GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.15f);
            PlayerManager.instance.player.GetComponent<SpriteRenderer>().color = playerColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
        invincible = false;
    }
}
