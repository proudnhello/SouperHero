using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/DamageBuff")]
public class DamageBuff : AbilityAbstractClass
{
    private GameObject player;
    [SerializeField] public int buffAmount;
    
    public override void Initialize(int duration)
    {
        player = PlayerManager.instance.player;

        // TODO: make buffAmount based on soupValue
        // ex: buffAmount = Mathf.CeilToInt(PlayerManager.instance.soupVal / 25)

        if (player != null)
        {
            player.GetComponent<PlayerAttack>().playerDamage += buffAmount;
            // TODO: make maxUsage based on soupValue
            // ex: _maxUsage = soupVal / 10
            _remainingUsage = _maxUsage;
            Debug.Log("buffing player!");
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
            End();
        }
        _remainingUsage--;
        Debug.Log("using buff >:]");
    }
    public override void End()
    {
        // decrease player damage by buff amount
        player.GetComponent<PlayerAttack>().playerDamage -= buffAmount;
        Debug.Log("debuffing player :(");
    }
}
