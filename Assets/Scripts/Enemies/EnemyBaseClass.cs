using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseClass : MonoBehaviour
{
    protected bool soupable = false;
    protected bool takingDamage = false;
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth = 100;
    internal float moveSpeed = 1f;
    internal SpriteRenderer sprite;
    protected Transform playerTransform;
    [SerializeField] protected String enemyName = "null";
    [SerializeField] protected int soupNumber = -1;
    protected Rigidbody2D _rigidbody;
    protected Color _initialColor;
    public int playerCollisionDamage = 10;
    [SerializeField] protected float knockBackTime = 1.0f;

    // initialize enemy status effect class
    internal EnemyStatusEffects statusEffect;

    protected void Start(){
        sprite = GetComponent<SpriteRenderer>();
        playerTransform = PlayerManager.instance.player.transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialColor = sprite.color;
        currentHealth = maxHealth;

        // make an instance of status effect class on startup
        statusEffect = new EnemyStatusEffects(this);
    }

    protected void Update(){
        if(!soupable && !takingDamage)
        {
            UpdateAI();
        } else if (soupable)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = 0.0f;
        }
        if(Input.GetKeyDown(KeyCode.G)){
            Debug.Log(Soupify());
        }
    }

    public int getCurrentHealth(){
        return currentHealth;
    }

    public bool getSoupable(){
        return soupable;
    }
    public float GetKnockBackTime()
    {
        return knockBackTime;
    }
    protected abstract void UpdateAI();
    protected void BecomeSoupable(){
        soupable = true;
        GetComponent<Collider2D>().isTrigger = true;
        sprite.color = sprite.color / 1.5f;
    }
    public void TakeDamage(int amount, GameObject source){
        if (!takingDamage)
        {
            takingDamage = true;
            currentHealth = Math.Clamp(currentHealth - amount, 0, maxHealth);
            if (currentHealth == 0)
            {
                BecomeSoupable();
            }
            else
            {
                Vector3 direction = (transform.position - source.transform.position).normalized;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(direction * 6, ForceMode2D.Impulse);
                StartCoroutine("KnockBack", knockBackTime);
            }
        }
    }

    public void TakeDamage(int amount, GameObject source, float knockback)
    {
        if (!takingDamage)
        {
            takingDamage = true;
            currentHealth = Math.Clamp(currentHealth - amount, 0, maxHealth);
            if (currentHealth == 0)
            {
                BecomeSoupable();
            }
            else
            {
                Vector3 direction = (transform.position - source.transform.position).normalized;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(direction * 6, ForceMode2D.Impulse);
                StartCoroutine("KnockBack", knockback);
            }
        }
    }

    public void DamagePlayer(int damage) {
        PlayerHealth playerHealth = PlayerManager.instance.player.GetComponent<PlayerHealth>();
        
        // Check if enemy is not soupable and player is not invincible
        if (!soupable && !playerHealth.IsInvincible()) {
            PlayerManager.instance.TakeDamage(damage);
            playerHealth.StartCoroutine(playerHealth.TakeDamageAnimation());
        }
    }

    public IEnumerator KnockBack(float time)
    {
        int maxFlashCycles = Mathf.CeilToInt((time / 0.3f));
        int flashCycles = 0;
        while(maxFlashCycles > flashCycles)
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sprite.color = _initialColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
        takingDamage = false;
    }

    public (String, int) Soupify(){
        if(soupable){
            Destroy(gameObject);
            return (enemyName, soupNumber);
        }
        else{
            return ("null", -1);
        }
    }

}
