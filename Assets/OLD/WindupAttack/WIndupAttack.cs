using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Abilities/WindupAttack")]
//public class WindupAttack : AbilityAbstractClass
//{
//    private PlayerManager player;
//    [SerializeField] private int maxUsageMult = 2;
//    [SerializeField] private float damageBuff = 20;
//    [SerializeField] private float delay = 0.5f;
//    [SerializeField] private float sizeIncreace = 1;

//    public override void Initialize(int soupVal)
//    {
//        player = PlayerManager.instance;

//        int usageValue = Mathf.CeilToInt(soupVal / maxUsageMult);

//        if (player != null)
//        {
//            Debug.Log("Windup attack buff initiated");
//            player.setAttackDelay(player.getAttackDelay() + delay);
//            player.SetDamage(player.GetDamage() + (int)damageBuff);
//            player.SetAttackRadius(player.GetAttackRadius() + sizeIncreace);
//        }
//        else
//        {
//            Debug.LogWarning("Player not found!");
//        }
//        return;
//    }
//    public override void UseAbility(){
        
//        Debug.Log("using windup attack");
        
//    }
//    public override void End()
//    {
//        // decrease player damage by buff amount
//        if (player != null)
//        {
//            player.setAttackDelay(player.getAttackDelay() - delay);
//            player.SetDamage(player.GetDamage() - (int)damageBuff);
//            player.SetAttackRadius(player.GetAttackRadius() - sizeIncreace);

//            //PlayerManager.instance.RemoveAbility(this);
//        }
//        else
//        {
//            Debug.LogWarning("Player not found!");
//        }
//    }
//}
