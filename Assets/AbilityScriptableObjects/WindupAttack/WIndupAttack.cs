using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/WindupAttack")]
public class WindupAttack : AbilityAbstractClass
{
    private PlayerManager player;
    [SerializeField] private int maxUsageMult = 2;
    [SerializeField] private int damageMult = 4;
    [SerializeField] private float delayMult = 10;
    [SerializeField] private float sizeMult = 100;


    int damageBuffAmount = 0;
    float delayAmount = 0;
    float sizeIncrease = 0;

    public override void Initialize(int soupVal)
    {
        player = PlayerManager.instance;

        int usageValue = Mathf.CeilToInt(soupVal / maxUsageMult);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;
        damageBuffAmount = soupVal/damageMult;
        delayAmount = soupVal/delayMult;
        sizeIncrease = soupVal/sizeMult;

        Debug.Log("Windup attack initialized");
        Debug.Log("Damage buff amount: " + damageBuffAmount);
        Debug.Log("Delay amount: " + delayAmount);
        Debug.Log("Size increase: " + sizeIncrease);
        

        if (player != null)
        {
            Debug.Log("Windup attack buff initiated");
            player.setAttackDelay(player.getAttackDelay() + 0.3f);
            player.SetDamage(player.GetDamage() + damageBuffAmount);
            player.SetAttackRadius(player.GetAttackRadius() + sizeIncrease);
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
            player.setAttackDelay(player.getAttackDelay() - 0.3f);
            player.SetDamage(player.GetDamage() - damageBuffAmount);
            player.SetAttackRadius(player.GetAttackRadius() - sizeIncrease);

            PlayerManager.instance.RemoveAbility(this);
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }
}
