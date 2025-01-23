using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/WindupAttack")]
public class WindupAttack : AbilityAbstractClass
{
    private PlayerManager player;
    [SerializeField] public int buffAmount; // this will change to be based on soup value
    
    public override void Initialize(int soupVal)
    {
        player = PlayerManager.instance;

        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        Debug.Log(usageValue);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;

        if (player != null)
        {
            Debug.Log("Windup attack initiated");
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
        return;
    }
    public override void Active(){
        if (_remainingUsage <= 0)
        {
            Debug.Log("ending windup attack");
            End();
        }
        else
        {
            _remainingUsage--;
            Debug.Log("using windup attack");
        }
    }
    public override void End()
    {
        // decrease player damage by buff amount
        if (player != null)
        {
            player.SetDamage(PlayerManager.instance.GetDamage() - buffAmount);
            Debug.Log("DMG after debuff: " + PlayerManager.instance.GetDamage());
            PlayerManager.instance.RemoveAbility(this);
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }
}
