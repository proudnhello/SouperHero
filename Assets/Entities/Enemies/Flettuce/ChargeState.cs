using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using static FlavorIngredient.InflictionFlavor;

public class ChargeState : BaseState
{
    FlettuceAI sm;
    public ChargeState(FlettuceAI _sm)
    {
        sm = _sm;
    }

    public override void DoActions()
    {
        throw new System.NotImplementedException();
    }

    public override void EnterState()
    {
        sm.StartCoroutine(Charge());
        Debug.Log("Entering Approach State");
    }

    public override void ExitState()
    {
        sm.StartCoroutine(Charge());
    }

    public override void SwitchHandling()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator Charge()
    {
        // PERFORM CHARGES
        sm.animator.Play("Attack");
        for (int chargeNum = 1; chargeNum <= sm.ConsecutiveCharges; chargeNum++)
        {
            yield return new WaitUntil(() => !sm.inflictionHandler.IsAfflicted(InflictionType.GREASY_Knockback));
            Vector2 vel = (sm.PlayerTransform.position - sm.transform.position).normalized * sm.ChargeForce * sm.GetMoveSpeed();
            for (float chargeTime = 0; chargeTime < sm.ChargeTime; chargeTime += Time.deltaTime)
            {
                if (sm._rigidbody.velocity.magnitude < sm.ChargeSpeed) sm._rigidbody.AddForce(vel * Time.deltaTime);
                yield return null;
            }

            if (chargeNum < sm.ConsecutiveCharges) yield return new WaitForSeconds(Random.Range(sm.ChargeCooldownTime.x, sm.ChargeCooldownTime.y));
            else yield return new WaitForSeconds(sm.FinalChargeCooldownTime);
        }
    }
}
