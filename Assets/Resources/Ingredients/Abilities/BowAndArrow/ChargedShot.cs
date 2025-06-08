using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Infliction = FinishedSoup.SoupInfliction;

[CreateAssetMenu(menuName = "Abilities/Charged Shot")]

public class ChargedShot : AbilityAbstractClass
{
    [SerializeField] ChargeIndicator chargeIndicator;
    [SerializeField] float baseChargeTime = 2f;
    ChargeIndicator currentChargeIndicator = null;
    [SerializeField] Sprite[] projectileFrames;
    [SerializeField] float projectileAnimFPS;
    [SerializeField] ProjectileSpawner spawner;
    float chargeTime = 0f;
    float maxChargeTime = 2f;

    protected override void Press(AbilityStats stats, List<Infliction> inflictions)
    {
        //base.Press(stats, inflictions);
        UnityEngine.Debug.Log("Charged Shot Pressed");
        if (currentChargeIndicator == null)
        {
            currentChargeIndicator = Instantiate(chargeIndicator.gameObject, PlayerEntityManager.Singleton.playerAttackPoint.position, PlayerEntityManager.Singleton.playerAttackPoint.rotation).GetComponent<ChargeIndicator>();
            currentChargeIndicator.transform.SetParent(PlayerEntityManager.Singleton.playerAttackPoint);
        }
        currentChargeIndicator.gameObject.SetActive(true);
        currentChargeIndicator.UpdateChargePercentage(0);
        chargeTime = 0;
        maxChargeTime = baseChargeTime / stats.speed;
        if (stats.speed < 1)
        {
            maxChargeTime = baseChargeTime;
        }
    }

    protected override void Hold(AbilityStats stats, List<Infliction> inflictions)
    {
        //base.Hold(stats, inflictions);
        chargeTime += Time.deltaTime;
        currentChargeIndicator.UpdateChargePercentage(chargeTime / maxChargeTime);
    }

    protected override void Release(AbilityStats stats, List<Infliction> inflictions)
    {
        //base.Release(stats, inflictions);
        if (chargeTime >= maxChargeTime)
        {
            // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
            ProjectileObject projectileObject = spawner.GetProjectile();
            AbilityStats projStats = stats;
            if (stats.speed < 1)
            {
                projStats.speed = 2f;
            }
            if (stats.duration < 1)
            {
                projStats.duration = 2f;
            }
            if (stats.size < 1)
            {
                projStats.size = 2f;
            }
            projStats.speed *= 20f;

            projectileObject.Spawn(PlayerEntityManager.Singleton.playerAttackPoint.position,
                               PlayerEntityManager.Singleton.playerAttackPoint.transform.up,
                                              projStats, inflictions, projectileFrames, projectileAnimFPS);
        }
        currentChargeIndicator.UpdateChargePercentage(0);
        currentChargeIndicator.gameObject.SetActive(false);
        if (currentChargeIndicator != null)
        {
            currentChargeIndicator.gameObject.SetActive(false);
        }
    }
}
