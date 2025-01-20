using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private GameObject testAttack; //Temporary object
    [SerializeField] private float attackRadius;
    void Start()
    {
        testAttack.SetActive(false); //Testing
        foreach (AbilityAbstractClass ability in PlayerManager.instance.GetAbilities()) //Initialize all abilities in array
        {
            ability.Initialize(10);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(PlayerManager.instance.attackKey))
        {
            Attack();
        }
        if (Input.GetKeyDown(PlayerManager.instance.soupKey))
        {
            SoupAttack();
        }
        if (Input.GetKeyDown(PlayerManager.instance.drinkey))
        {
            print("Drink");
            PlayerManager.instance.Drink();
        }
    }

    void Attack()
    {
        StartCoroutine(TestDisplayPlayerAttack());

        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, PlayerManager.instance.GetEnemies());
        foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
        {
            enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().TakeDamage(PlayerManager.instance.GetDamage());
        }

        foreach (AbilityAbstractClass ability in PlayerManager.instance.GetAbilities().ToList()) //Activate all abilities in array
        {
            ability.Active();
        }
    }

    void SoupAttack()
    {
        StartCoroutine(TestDisplayPlayerAttack());

        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, PlayerManager.instance.GetEnemies());
        foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
        {
            (string, int) soup = enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().Soupify();
            if (soup.Item1 != null && soup.Item1 != "null")
            {
                PlayerManager.instance.AddToPot(soup);
            }
        }
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
