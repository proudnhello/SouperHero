using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Shield")]
public class Shield : AbilityAbstractClass
{
    private PlayerManager player;

    public override void Initialize(int soupVal)
    {
        player = PlayerManager.instance;
        player.setShieldAmount(soupVal);
    }
    public override void Active()
    {
        return;
    }

    public override void End()
    {
        player.setShieldAmount(0);
    }
}
