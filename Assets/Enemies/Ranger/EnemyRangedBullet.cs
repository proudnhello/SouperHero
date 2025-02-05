using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bullet class that is called when enemy bullet is instantiated from EnemyRanged
public class EnemyRangedBullet : MonoBehaviour
{
    private float bulletLifeTime = 3f;
    private float bulletSpeed = 15f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(this.gameObject, bulletLifeTime);
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.up * bulletSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collider) //Lo: This is temporary until the DamagePlayer() function in EnemyBaseClass is complete
    {
        if (collider.gameObject.tag == "Player")
        {
            PlayerManager.instance.TakeDamage(10, this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
