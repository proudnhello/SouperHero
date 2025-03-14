using UnityEngine;
using System.Collections;

public class EntityRenderer
{
    protected Entity Entity;
    protected SpriteRenderer spriteRenderer;
    public EntityRenderer(Entity entity)
    {
        Entity = entity;
        spriteRenderer = Entity.GetComponent<SpriteRenderer>();
    }

    public IEnumerator TakeDamageAnimation()
    {
        float maxFlashCycles = (Entity.GetInvincibility() / 0.3f);
        int flashCycles = 0;
        Color normalColor = spriteRenderer.color;

        while (maxFlashCycles > flashCycles)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = normalColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
        spriteRenderer.color = normalColor;
    }

    float deathAnimTime = 1.0f;
    public IEnumerator EnemyDeathAnimation()
    {
        float timeProgressed = deathAnimTime;
        Color normalColor = spriteRenderer.color;
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