using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallCollision : MonoBehaviour
{
    private const int enemyLayer = 7;
    [SerializeField] private bool despawn = true;
    [SerializeField] public float despawnTime = -1f;
    public AbilityAbstractClass.AbilityStats stats;
    public List<EntityStatusEffects.StatusEffect> statusEffects;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == enemyLayer)
        {
            EnemyBaseClass enemy =  collider.gameObject.GetComponent<EnemyBaseClass>();
            enemy.TakeDamage(stats.damage, this.gameObject);
            enemy.AddStatusEffects(statusEffects);
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
