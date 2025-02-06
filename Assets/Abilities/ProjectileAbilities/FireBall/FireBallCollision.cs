using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallCollision : MonoBehaviour
{
    private const int enemyLayer = 7;
    [SerializeField] private bool despawn = true;
    [SerializeField] public float despawnTime = -1f;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == enemyLayer)
        {
            EnemyBaseClass enemy =  collider.gameObject.GetComponent<EnemyBaseClass>();
            enemy.TakeDamage(PlayerManager.instance.GetDamage(), this.gameObject);
            EntityStatusEffects.StatusEffect burn = EntityStatusEffects.CreateStatusEffect(EntityStatusEffects.StatusType.Burn, 2f, 0.2f, 1);
            enemy.AddStatusEffect(burn);
            
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
