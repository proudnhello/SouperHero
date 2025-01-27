using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/MineAttack")]

public class MineAttack : AbilityAbstractClass
{
    private PlayerManager player;
    [SerializeField] GameObject minePrefab;
    [SerializeField] private int maxUsageMult = 2;
    [SerializeField] private int damageMult = 4;
    [SerializeField] private float delayMult = 3;
    [SerializeField] private float sizeMult = 1.5f;
    [SerializeField] private float mineLifespan = 5f;

    private GameObject currentMine;

    public override void Initialize(int soupVal)
    {
        Debug.Log("MineAttack initialized");
        _maxUsage = Mathf.CeilToInt(soupVal / 2.0f);
        _remainingUsage = _maxUsage;
    }

    public override void Active()
    {
        _remainingUsage--;

        if (currentMine == null)
        {
            currentMine = Instantiate(minePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
            currentMine.transform.position = PlayerManager.instance.player.transform.position;
            currentMine.transform.localScale = new Vector3(sizeMult, sizeMult, sizeMult);
        }

        currentMine.GetComponent<MineDamage>().despawnTime = mineLifespan;

        Debug.Log("MineAttack active");

        if (_remainingUsage <= 0)
        {
            End();
        }
    }

    public override void End()
    {
        Debug.Log("MineAttack ended");
        PlayerManager.instance.RemoveAbility(this);
    }

}
