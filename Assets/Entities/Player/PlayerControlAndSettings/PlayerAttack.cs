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
        foreach (AbilityAbstractClass ability in PlayerManager.instance.GetAbilities()) //Initialize all abilities in array
        {

        }
    }

    void Update()
    {
        if (Input.GetKeyDown(PlayerManager.instance.attackKey) || Input.GetKeyDown(PlayerManager.instance.altAttackKey))
        {
            Attack();
        }
        if (Input.GetKeyDown(PlayerManager.instance.soupKey) || Input.GetKeyDown(PlayerManager.instance.altSoupKey))
        {
            SoupAttack();
        }
        KeyCode currentKey = KeyCode.Alpha1;
        // Due to how the key codes are arranged, adding 1 to Alpha1 will give us Alpha2, and so on
        // Things get icky if we have 10 or more pots, but ah well sure 
        for (int i = 0; i < PlayerManager.instance.GetNumberOfPots(); i++)
        {
            if (Input.GetKeyDown(currentKey))
            {
                mostRecentPotUsed = i;
                PlayerManager.instance.Drink(i);
            }
            currentKey++;
        }

        attackRadius = PlayerManager.instance.GetAttackRadius();
        testAttack.transform.localScale = new Vector3(attackRadius, attackRadius, 1);

        if (isAttacking)
        {
            Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, PlayerManager.instance.GetEnemies());
            foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
            {
                enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().TakeDamage(PlayerManager.instance.GetDamage(), this.gameObject);
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
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, PlayerManager.instance.GetEnemies());
        foreach (Collider2D enemyGameObject in enemy) //Check if enemy is in attackRadius
        {
            PlayerSoup.FlavorIngredient soup = enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().Soupify();
            if(soup.name != "null") {
                PlayerManager.instance.AddToInventory(soup);
            }
        }
    }

    IEnumerator TestDisplayPlayerAttack() //Display test attack radius
    {
        // Windup
        yield return new WaitForSeconds(PlayerManager.instance.getAttackDelay());
        isAttacking = true;

        // Attack
        testAttack.SetActive(true);
        if (!souping)
        {
            foreach (AbilityAbstractClass ability in PlayerManager.instance.GetAbilities().ToList()) //Activate all abilities in array
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
        yield return new WaitForSeconds(1f/PlayerManager.instance.GetAttackSpeed());
        testAttack.SetActive(false);
        isAttacking = false;
        souping = false;
    }

    private void OnDrawGizmos() //Draw in scene for testing
    {
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRadius); //Display attack radius
    }
}
