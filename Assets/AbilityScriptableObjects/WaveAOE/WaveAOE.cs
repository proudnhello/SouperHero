using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/WaveAOE")]
public class WaveAOE : AbilityAbstractClass
{
    [SerializeField] GameObject wavePrefab;
    [SerializeField] float waveLifespan = 1f;
    [SerializeField] float waveScale = 1f;
    GameObject currentWave = null;
    public override void Initialize(int soupVal)
    {
        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;
    }
    public override void Active()
    {
        if (currentWave != null)
        {
            currentWave = Instantiate(wavePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
            currentWave.transform.parent = PlayerManager.instance.player.transform;
            currentWave.transform.localScale = new Vector3(waveScale, waveScale, waveScale);
        }
    }

    public override void End()
    {
        Debug.Log("Ability1 Example End");
    }
    
}
