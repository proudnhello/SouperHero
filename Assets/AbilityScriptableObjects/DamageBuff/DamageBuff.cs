using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/DamageBuff")]
public class DamageBuff : AbilityAbstractClass
{
    private GameObject player;
    [SerializeField] public int buffAmount; // this will change to be based on soup value
    
    public override void Initialize(int duration)
    {
        player = PlayerManager.instance.player;
        // TODO: make buffAmount based on soupValue
        // ex: buffAmount = Mathf.CeilToInt(PlayerManager.instance.soupVal / 25)

        int usageValue = Mathf.CeilToInt(duration / 10.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;

        if (player != null)
        {
            Debug.Log("DMG Before buff: " + player.GetComponent<PlayerAttack>().playerDamage);
            player.GetComponent<PlayerAttack>().playerDamage += buffAmount;
            Debug.Log("DMG after buff: " + (player.GetComponent<PlayerAttack>().playerDamage));
            // TODO: make maxUsage based on soupValue
            // ex: _maxUsage = soupVal / 10
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
            Debug.Log("using buff >:], buffed DMG = " + player.GetComponent<PlayerAttack>().playerDamage);
        }
    }
    public override void End()
    {
        // decrease player damage by buff amount
        if (player != null)
        {
            player.GetComponent<PlayerAttack>().playerDamage -= buffAmount;
            Debug.Log("DMG after debuff: " + (player.GetComponent<PlayerAttack>().playerDamage));
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }
}
