using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBuff : AbilityAbstractClass
{
    GameObject player = PlayerManager.instance.player;
    float buffAmount;
    
    public override void Initialize(int duration)
    {
        // buffAmount = Mathf.CeilToInt(PlayerManager.instance.soupVal / 25)
        if (player != null)
        {
            player.GetComponent<PlayerAttack>().playerDamage += buffAmount;
            // _maxUsage = soupVal / 10
            _remainingUsage = _maxUsage;
        }

        // increase player damage by soupVal/25 rounded up
        return;
    }
    public override void Active(){
        if (_remainingUsage <= 0)
        {
            End();
        }
        _remainingUsage -= 1;
    }
    public override void End()
    {
        // decrease player damage by soupVal/25
        player.GetComponent<PlayerAttack>().playerDamage -= buffAmount;
    }
}
