using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    private const int enemyLayer = 7;
    [SerializeField] private bool despawn = true;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == enemyLayer)
        {
            collider.gameObject.GetComponent<EnemyBaseClass>().TakeDamage(PlayerManager.instance.GetDamage(), this.gameObject);
            if (despawn)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
