using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/WaveAOE")]
public class WaveAOE : AbilityAbstractClass
{
    [SerializeField] GameObject wavePrefab;
    GameObject currentWave;
    public override void Initialize(int soupVal)
    {
        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;
    }
    public override void Active()
    {
        currentWave = Instantiate(wavePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
    }

    public override void End()
    {
        Debug.Log("Ability1 Example End");
    }
    
}
