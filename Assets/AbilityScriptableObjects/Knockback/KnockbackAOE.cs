using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/KnockbackAOE")]
public class KnockbackAOE : AbilityAbstractClass
{
    [SerializeField] GameObject wavePrefab;
    [SerializeField] float waveLifespan = 1f;
    [SerializeField] float playerAttackScale = 1f;
    int usageValue = 5;
    GameObject currentWave = null;
    public override void Initialize(int soupVal)
    {
        usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;
    }
    public override void Active()
    {
        if (currentWave == null)
        {
            currentWave = Instantiate(wavePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
            currentWave.transform.parent = PlayerManager.instance.player.transform;
            float waveScale = PlayerManager.instance.GetAttackRadius() * playerAttackScale;
            currentWave.transform.localScale = new Vector3(waveScale, waveScale, waveScale);

        }

        currentWave.GetComponent<KnockbackEnemy>().despawnTime = waveLifespan;

        usageValue--;
        if(usageValue <= 0)
        {
            End();
        }
    }

    public override void End()
    {
        currentWave.GetComponent<KnockbackEnemy>().despawnTime = 0;
        PlayerManager.instance.RemoveAbility(this);
    }
    
}
