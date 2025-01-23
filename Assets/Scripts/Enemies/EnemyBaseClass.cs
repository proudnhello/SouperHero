using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseClass : MonoBehaviour
{
    protected bool soupable = false;
    protected bool takingDamage = false;
    protected int maxHealth = 100;
    protected int currentHealth = 100;
    protected float moveSpeed = 1f;
    protected SpriteRenderer sprite;
    protected Transform playerTransform;
    protected String soupAbility = "null";
    protected int soupNumber = -1;
    protected Rigidbody2D _rigidbody;
    protected Color _initialColor;

    [SerializeField]
    protected float knockBackTime = 1.0f;

    protected void Start(){
        sprite = GetComponent<SpriteRenderer>();
        playerTransform = PlayerManager.instance.player.transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialColor = sprite.color;
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
    protected abstract void UpdateAI();
    protected void BecomeSoupable(){
        soupable = true;
        sprite.color = new Color(255, 255, 255);
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
                StartCoroutine("KnockBack");
            }
        }
    }

    public void DamagePlayer() {
        
    }

    public IEnumerator KnockBack()
    {
        int maxFlashCycles = Mathf.CeilToInt((knockBackTime / 0.3f));
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
            return (soupAbility, soupNumber);
        }
        else{
            return ("null", -1);
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Bullet"))
        {

        }
    }
}
