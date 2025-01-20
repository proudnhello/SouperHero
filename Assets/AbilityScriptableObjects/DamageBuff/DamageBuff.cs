using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/DamageBuff")]
public class DamageBuff : AbilityAbstractClass
{
    private PlayerManager player;
    [SerializeField] public int buffAmount; // this will change to be based on soup value
    
    public override void Initialize(int soupVal)
    {
        player = PlayerManager.instance;
        // TODO: make buffAmount based on soupValue
        // ex: buffAmount = Mathf.CeilToInt(PlayerManager.instance.soupVal / 25)

        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        Debug.Log(usageValue);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;

        if (player != null)
        {
            Debug.Log("DMG Before buff: " + PlayerManager.instance.GetDamage());
            player.SetDamage(buffAmount + PlayerManager.instance.GetDamage());
            Debug.Log("DMG after buff: " + PlayerManager.instance.GetDamage());
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
            Debug.Log("ending buff");
            End();
        }
        else
        {
            _remainingUsage--;
            Debug.Log("using buff >:], buffed DMG = " + PlayerManager.instance.GetDamage());
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
