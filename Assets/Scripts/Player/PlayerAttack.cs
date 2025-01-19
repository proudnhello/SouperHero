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
    [SerializeField] public int playerDamage = 10;

    [SerializeField] private AbilityAbstractClass[] abilities; //Lo: This will likely be moved later.
                                                               //Added all abilities for testing
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
            enemyGameObject.gameObject.GetComponent<EnemyBaseClass>().TakeDamage(playerDamage);
        }

        foreach (AbilityAbstractClass ability in abilities) //Activate all abilities in array
        {
            ability.Active();
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
