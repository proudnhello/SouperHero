// portions of this file were generated using GitHub Copilot
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
    [SerializeField] Sprite[] ProjectileFrames;
    [SerializeField] float ProjectileAnimFPS;

    private Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Destroy(gameObject, bulletLifeTime);
        rb.velocity = direction * bulletSpeed;
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        StartCoroutine(HandleAnimation());
    }

    public void SetDirection(Vector2 d)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        rb.velocity = Vector2.zero;
        direction = d.normalized;
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
        }else if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();
            if (entity != null)
            {
                entity.DealDamage(bulletDamage);
            }
        }

        if (collider.gameObject.tag != "PitHazard")
        {
            Destroy(this.gameObject);
        }   
    }

    IEnumerator HandleAnimation()
    {
        int frame = 0;
        while (true && ProjectileFrames.Length != 0)
        {
            spriteRenderer.sprite = ProjectileFrames[frame];
            yield return new WaitForSeconds(1f / ProjectileAnimFPS);
            frame = (frame + 1) % ProjectileFrames.Length;
        }
    }
}
