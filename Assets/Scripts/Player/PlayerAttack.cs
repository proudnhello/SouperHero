using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Keybinds")]
    private KeyCode attackKey = KeyCode.Mouse0;

    [Header("Attack")]
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private GameObject testAttack; //Temporary object
    [SerializeField] private float attackRadius;
    [SerializeField] private LayerMask enemies;

    void Start()
    {
        testAttack.SetActive(false); //Testing
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        StartCoroutine(TestDisplayPlayerAttack());

        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, enemies);
        foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
        {
            enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().TakeDamage(10);
        }

        //Lo TODO: Iterate over ability array and activate() all abilities
    }

    IEnumerator TestDisplayPlayerAttack() //Display test attack radius
    {
        testAttack.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        testAttack.SetActive(false);
    }

    private void OnDrawGizmos() //Draw in scene for testing
    {
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRadius); //Display attack radius
    }
}
