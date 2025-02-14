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
    private bool isAttacking;
    private bool souping = false;
    int mostRecentPotUsed = 0;
    void Start()
    {
        testAttack.SetActive(false); //Testing
        foreach (AbilityAbstractClass ability in PlayerManager.Singleton.UseSpoon()) //Initialize all abilities in array
        {

        }
    }

    void Update()
    {
        if (Input.GetKeyDown(PlayerManager.Singleton.attackKey) || Input.GetKeyDown(PlayerManager.Singleton.altAttackKey))
        {
            Attack();
        }
        if (Input.GetKeyDown(PlayerManager.Singleton.soupKey) || Input.GetKeyDown(PlayerManager.Singleton.altSoupKey))
        {
            SoupAttack();
        }
        KeyCode currentKey = KeyCode.Alpha1;
        // Due to how the key codes are arranged, adding 1 to Alpha1 will give us Alpha2, and so on
        // Things get icky if we have 10 or more pots, but ah well sure 
        for (int i = 0; i < PlayerManager.Singleton.GetNumberOfPots(); i++)
        {
            if (Input.GetKeyDown(currentKey))
            {
                mostRecentPotUsed = i;
                // set current spoon
                PlayerManager.Singleton.SetCurrentSpoon(i);
                // use current spoon
                PlayerManager.Singleton.UseSpoon();
            }
            currentKey++;
        }

        attackRadius = PlayerManager.Singleton.GetAttackRadius();
        testAttack.transform.localScale = new Vector3(attackRadius, attackRadius, 1);

        if (isAttacking)
        {
            Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, PlayerManager.Singleton.GetEnemies());
            foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
            {
                enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().TakeDamage(PlayerManager.Singleton.GetDamage(), this.gameObject);
            }
        }
    }

    void Attack()
    {
        if(!isAttacking){
            StartCoroutine(TestDisplayPlayerAttack());
        }
    }

    void SoupAttack()
    {
        if(!isAttacking){
            souping = true;
            StartCoroutine(TestDisplayPlayerAttack());
        }
        else
        {
            return;
        }
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, PlayerManager.Singleton.GetEnemies());
        foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
        {
            AbilityIngredient soup = enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().Soupify();
            if(soup != null && soup.ingredientName != "null") {
                PlayerManager.Singleton.AddToInventory(soup);
            }
        }
    }

    IEnumerator TestDisplayPlayerAttack() //Display test attack radius
    {
        // Windup
        yield return new WaitForSeconds(PlayerManager.Singleton.getAttackDelay());
        isAttacking = true;

        // Attack
        testAttack.SetActive(true);
        if (!souping)
        {
            foreach (AbilityAbstractClass ability in PlayerManager.Singleton.UseSpoon().ToList()) //Activate all abilities in array
            {
                ability.Active();

                //Printing The Ability to The Console
                //Debug.Log(ability);
            }
        }
        else
        {
            print("Skipping Abilities b/c SOUP");
        }
        yield return new WaitForSeconds(1f/PlayerManager.Singleton.GetAttackSpeed());
        testAttack.SetActive(false);
        isAttacking = false;
        souping = false;
    }

    private void OnDrawGizmos() //Draw in scene for testing
    {
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRadius); //Display attack radius
    }
}
