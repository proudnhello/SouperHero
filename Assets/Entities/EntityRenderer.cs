using UnityEngine;
using System.Collections;

public class EntityRenderer
{
    // Who the hell made the entity renderer work like this????
    // Why do we need an entirely seperate class to handles some sub functions of the entity???
    // Why is it not a component of the entity itself???
    // I come along trying to override the death animation for a boss and have to make an entirely new class just to do that??? Why???
    // Maybe there's some genious reason for this that I'm not seeing, but it seems like an unnecessary abstraction layer to me
    protected Entity Entity;
    protected SpriteRenderer spriteRenderer;
    public EntityRenderer(Entity entity)
    {
        Entity = entity;
        spriteRenderer = Entity.GetComponent<SpriteRenderer>();
    }

    IEnumerator ITakeDamageAnimation;
    public void TakeDamage()
    {
        if (ITakeDamageAnimation != null) Entity.StopCoroutine(ITakeDamageAnimation);
        Entity.StartCoroutine(ITakeDamageAnimation = TakeDamageAnimation());
    }

    IEnumerator TakeDamageAnimation()
    {
        float maxFlashCycles = (Entity.GetInvincibility() / 0.3f);
        int flashCycles = 0;

        while (maxFlashCycles > flashCycles)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
        spriteRenderer.color = Color.white;
    }

    IEnumerator IEnemyDeathAnimation;
    public void EnemyDeath()
    {
        if (IEnemyDeathAnimation != null) Entity.StopCoroutine(IEnemyDeathAnimation);
        Entity.StartCoroutine(IEnemyDeathAnimation = EnemyDeathAnimation());
    }

    protected float deathAnimTime = 1.0f;
    protected virtual IEnumerator EnemyDeathAnimation()
    {
        float timeProgressed = deathAnimTime;
        Color normalColor = Color.white;
        while (timeProgressed > 0)
        {
            normalColor.a = timeProgressed / deathAnimTime;
            spriteRenderer.color = normalColor;
            timeProgressed -= Time.deltaTime;
            yield return null;
        }

        Entity.Destroy(Entity.gameObject);
    }
}