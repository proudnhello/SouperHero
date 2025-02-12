using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/WaveAOE")]
public class WaveAOE : AbilityAbstractClass
{
    [SerializeField] GameObject wavePrefab;
    [SerializeField] float waveLifespan = 1f;
    [SerializeField] float waveScale = 1f;
    int usageValue = 5;
    GameObject currentWave = null;
    public override void Initialize(int soupVal)
    {
        usageValue = Mathf.CeilToInt(soupVal / 2.0f);
    }
    public override void Active()
    {
        Debug.Log("current wave" + currentWave);
        if (currentWave == null)
        {
            currentWave = Instantiate(wavePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
            currentWave.transform.parent = PlayerManager.instance.player.transform;
            currentWave.transform.localScale = new Vector3(waveScale, waveScale, waveScale);

        }

        currentWave.GetComponent<ProjectileDamage>().despawnTime = waveLifespan;

        usageValue--;
    }

    public override void End()
    {
        //PlayerManager.instance.RemoveAbility(this);
    }
    
}
