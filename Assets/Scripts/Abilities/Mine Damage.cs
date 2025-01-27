using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineDamage : MonoBehaviour
{
    private const int enemyLayer = 7;
    [SerializeField] private bool despawn = true;
    [SerializeField] public float despawnTime = -1f;

    [SerializeField] private float explosionTime = 0.5f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private LayerMask enemyLayerMask;
    
    // When an enemy enters the mine's trigger collider, the mine will explode
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == enemyLayer)
        {
            StartCoroutine(Explode());
            
        }
    }

    // timed enemy explosion that damages enemies within it's radius
    private IEnumerator Explode(){
        Color originalColor = this.gameObject.GetComponent<SpriteRenderer>().color;

        float maxFlashCycles = ((explosionTime / 0.3f));
        int flashCycles = 0;
        while(maxFlashCycles > flashCycles){
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.15f);
            this.gameObject.GetComponent<SpriteRenderer>().color = originalColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles ++;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(this.transform.position, explosionRadius, enemyLayerMask);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyBaseClass enemyBase = enemy.gameObject.GetComponent<EnemyBaseClass>();
            if(enemyBase != null){
                enemyBase.TakeDamage(PlayerManager.instance.GetDamage(), this.gameObject);
            }
        }

        if (despawn)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, explosionRadius);
    }

    // after a certain amount of time, the mine will explode even if an enemy hasn't triggered it
    private void FixedUpdate()
    {
        if (despawnTime > 0)
        {
            despawnTime -= Time.fixedDeltaTime;
            if (despawnTime <= 0)
            {
                StartCoroutine(Explode());
            }
        }   
    }

 
}
