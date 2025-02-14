using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

//[CreateAssetMenu(menuName = "Abilities/KnockbackAOE")]
//public class KnockbackAOE : AbilityAbstractClass
//{
//    [SerializeField] GameObject wavePrefab;
//    [SerializeField] float waveLifespan = 1f;
//    [SerializeField] float playerAttackScale = 1f;
//    [SerializeField] float damageToKnockback = 0.1f;
//    [SerializeField] float damageMult = 0.3f;
//    int usageValue = 5;
//    GameObject currentWave = null;
//    public override void Initialize(int soupVal)
//    {
//        usageValue = Mathf.CeilToInt(soupVal / 2.0f);
//    }
//    public override void UseAbility()
//    {
//        if (currentWave == null)
//        {
//            currentWave = Instantiate(wavePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
//            currentWave.transform.parent = PlayerManager.instance.player.transform;
//            float waveScale = PlayerManager.instance.GetAttackRadius() * playerAttackScale;
//            currentWave.transform.localScale = new Vector3(waveScale, waveScale, waveScale);
//            KnockbackEnemy knockback = currentWave.GetComponent<KnockbackEnemy>();
//            knockback.damageToKnockback = damageToKnockback;
//            knockback.damageMult = damageMult;
//        }

//        currentWave.GetComponent<KnockbackEnemy>().despawnTime = waveLifespan;
//    }

//    public override void End()
//    {
//        currentWave.GetComponent<KnockbackEnemy>().despawnTime = 0;
//        //PlayerManager.instance.RemoveAbility(this);
//    }
    
//}
