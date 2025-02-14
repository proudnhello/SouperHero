using UnityEngine;
using System.Collections;

public class PlayerRenderer
{
    PlayerEntityManager Player;
    SpriteRenderer spriteRenderer;
    public PlayerRenderer(PlayerEntityManager player)
    {
        Player = player;
        spriteRenderer = Player.GetComponent<SpriteRenderer>();
    }

    public IEnumerator TakeDamageAnimation()
    {
        float maxFlashCycles = ((Player.invincibility / 0.3f));
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
        Player.health.ResetInvincibility();
    }
}