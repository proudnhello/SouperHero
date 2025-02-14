using UnityEngine;
using System.Collections;

public class EntityRenderer
{
    Entity Entity;
    SpriteRenderer spriteRenderer;
    public EntityRenderer(Entity entity)
    {
        Entity = entity;
        spriteRenderer = Entity.GetComponent<SpriteRenderer>();
    }

    public IEnumerator TakeDamageAnimation()
    {
        float maxFlashCycles = ((Entity.GetInvincibility() / 0.3f));
        int flashCycles = 0;
        Color playerColor = spriteRenderer.color;

        while (maxFlashCycles > flashCycles)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = playerColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }
    }
}