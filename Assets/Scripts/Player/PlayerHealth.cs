using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private float damageTime = 1.0f;   // time the player flashes red when taking damage
    private bool takingDamage = false;
    void Start()
    {
        PlayerManager.instance.SetHealth(PlayerManager.instance.GetMaxHealth());
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7 && !takingDamage)    // 7 is the layer for enemy
        {
            takingDamage = true;
            KnockBack(collision.gameObject);
            StartCoroutine("TakeDamage");
        }
    }

    private void KnockBack(GameObject collider )
    {
        Debug.Log("Knocking back player");
        PlayerManager.instance.TakeDamage(10); // change this so that the player takes damage based on the enemy's damage
        Vector3 direction = (PlayerManager.instance.player.transform.position - collider.transform.position).normalized;
        PlayerManager.instance.player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        PlayerManager.instance.player.GetComponent<Rigidbody2D>().AddForce(direction * 3, ForceMode2D.Impulse);
    }

    public IEnumerator TakeDamage(){
        Debug.Log("Flashing damage indicator");

        int maxFlashCycles = Mathf.CeilToInt((damageTime / 0.3f));
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
        takingDamage = false;
    }

    /*
    public IEnumerator KnockBack()
    {
        int maxFlashCycles = Mathf.CeilToInt((knockBackTime / 0.3f));
        int flashCycles = 0;
        while(maxFlashCycles > flashCycles)
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sprite.color = _initialColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
        takingDamage = false;
    } 
    */
}
