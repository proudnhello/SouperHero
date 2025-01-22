using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private float damageTime = 1.0f;   // time the player flashes red when taking damage
    private bool invincible = false;
    void Start()
    {
        PlayerManager.instance.SetHealth(PlayerManager.instance.GetMaxHealth());
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !invincible)    
        {
            invincible = true;
            PlayerManager.instance.TakeDamage(10); // change this so that the player takes damage based on the enemy's damage
            KnockBack(collision.gameObject);
            StartCoroutine("TakeDamage");
        }
    }

    private void KnockBack(GameObject collider)
    {
        Vector3 direction = (PlayerManager.instance.player.GetComponent<Transform>().position - collider.GetComponent<Transform>().position).normalized;
        PlayerManager.instance.player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        PlayerManager.instance.player.GetComponent<Rigidbody2D>().AddForce(direction * 3, ForceMode2D.Impulse);
    }

    public IEnumerator TakeDamage(){

        float maxFlashCycles = ((damageTime / 0.3f));
        int flashCycles = 0;
        Color playerColor = PlayerManager.instance.player.GetComponent<SpriteRenderer>().color;
        
        while(maxFlashCycles > flashCycles)
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
