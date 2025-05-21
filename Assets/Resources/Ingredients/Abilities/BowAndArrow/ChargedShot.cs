using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Charged Shot")]

public class ChargedShot : AbilityAbstractClass
{
    [SerializeField] ChargeIndicator chargeIndicator;
    [SerializeField] GameObject projectile;
    [SerializeField] float baseChargeTime = 2f;
    ChargeIndicator currentChargeIndicator = null;
    float chargeTime = 0f;
    float maxChargeTime = 2f;

    protected override void Press(AbilityStats stats, List<SoupSpoon.SpoonInfliction> inflictions)
    {
        //base.Press(stats, inflictions);
        if (currentChargeIndicator == null)
        {
            currentChargeIndicator = Instantiate(chargeIndicator.gameObject, PlayerEntityManager.Singleton.playerAttackPoint.position, Quaternion.identity).GetComponent<ChargeIndicator>();
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

    protected override void Hold(AbilityStats stats, List<SoupSpoon.SpoonInfliction> inflictions)
    {
        //base.Hold(stats, inflictions);
        chargeTime += Time.deltaTime;
        chargeIndicator.UpdateChargePercentage(chargeTime / maxChargeTime);
    }

    protected override void Release(AbilityStats stats, List<SoupSpoon.SpoonInfliction> inflictions)
    {
        //base.Release(stats, inflictions);
        if (chargeTime >= maxChargeTime)
        {
            // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
            Debug.Log("Charged Shot Released");
        }
        currentChargeIndicator.UpdateChargePercentage(0);
        currentChargeIndicator.gameObject.SetActive(false);
        if (currentChargeIndicator != null)
        {
            currentChargeIndicator.gameObject.SetActive(false);
        }
    }
}
