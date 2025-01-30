using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackEnemy : MonoBehaviour
{
    private const int enemyLayer = 7;
    [SerializeField] private bool despawn = true;
    [SerializeField] public float despawnTime = -1f;
    [SerializeField] private float damageToKnockback = 0.1f;
    [SerializeField] private float damageMult = 0.3f;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == enemyLayer)
        {
            EnemyBaseClass enemy = collider.gameObject.GetComponent<EnemyBaseClass>();
            enemy.TakeDamage((int)(PlayerManager.instance.GetDamage() * damageMult), this.gameObject, enemy.GetKnockBackTime() * PlayerManager.instance.GetDamage() * damageToKnockback);
            if (despawn)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (despawnTime > 0)
        {
            despawnTime -= Time.fixedDeltaTime;
            if (despawnTime <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
