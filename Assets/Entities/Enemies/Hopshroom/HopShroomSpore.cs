using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bullet class that is called when enemy bullet is instantiated from EnemyRanged
public class HopShroomSpore : MonoBehaviour
{
    public float bulletLifeTime = 3f;
    public float bulletSpeed = 15f;
    public int bulletDamage = 10;
    public Vector2 direction;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, bulletLifeTime);
        rb.velocity = direction * bulletSpeed;
    }


    private void OnTriggerEnter2D(Collider2D collider) //Lo: This is temporary until the DamagePlayer() function in EnemyBaseClass is complete
    {
        if (collider.gameObject.tag == "Player")
        {
            PlayerEntityManager.Singleton.DealDamage((int)bulletDamage);       
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
        }
        Destroy(this.gameObject);
    }
}
