using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using static FlavorIngredient.InflictionFlavor;

public class ConsecutiveChargeState : BaseState
{
    FlettuceAI _blackboard;

    public ConsecutiveChargeState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
    }

    public override void DoActions()
    {
        ////yield return new WaitUntil(() => !_blackboard.inflictionHandler.IsAfflicted(InflictionType.GREASY_Knockback));


        //Vector2 vel = (_blackboard.PlayerTransform.position - _blackboard.transform.position).normalized * _blackboard.ChargeForce * _blackboard.GetMoveSpeed();
        //if (_blackboard.ChargeTime > Mathf.Abs(_chargeUpStartTime - Time.time)) {
        //    if (_blackboard._rigidbody.velocity.magnitude < _blackboard.ChargeSpeed)
        //    {
        //        _blackboard._rigidbody.AddForce(vel * Time.deltaTime);
        //    }
        //    return;
        //}

        //// Start timer if it hasn't started yet
        //if (!_blackboard.HasTimerStarted())
        //{
        //    _blackboard.StartWaitTimer(Random.Range(_blackboard.ChargeCooldownTime.x, _blackboard.ChargeCooldownTime.y));
        //    return;
        //}
        //else 
        //{
        //    // Skip rest of timestep if timer isn't up
        //    if (!_blackboard.CheckWaitTimer())
        //    {
        //        return;
        //    }    
        //}

        //if (_chargeNum < _blackboard.ConsecutiveCharges)
        //{
        //    _chargeNum++;
        //    _chargeUpStartTime = Time.time;
        //}
        //else
        //{

        //    // WAIT THE LONG COOLDOWN AT THE END OF THE CHAIN OF CHARGES

        //    // Start timer if it hasn't started yet
        //    if (!_blackboard.HasTimerStarted())
        //    {
        //        _blackboard.StartWaitTimer(_blackboard.FinalChargeCooldownTime);
        //        return;
        //    }
        //    else
        //    {
        //        // Skip rest of timestep if timer isn't up
        //        if (_blackboard.CheckWaitTimer())
        //        {
        //            Debug.Log("Long timer not up");
        //            return;
        //        }
        //        else
        //        {
        //            // Final Else Statement Here
        //            Debug.Log("END CHARGE STATE IS TRUE");
        //            _endChargeState = true;
        //        }
        //    }
        //}


        ////if (chargeNum < _blackboard.ConsecutiveCharges) yield return new WaitForSeconds(Random.Range(_blackboard.ChargeCooldownTime.x, _blackboard.ChargeCooldownTime.y));
        ////else yield return new WaitForSeconds(_blackboard.FinalChargeCooldownTime);
        ///
    }

    public override void EnterState()
    {
        _blackboard.animator.Play("Attack");
        machine.SetState(_blackboard.stateFactory.Charge(), this);
        _blackboard.ChargesCounter = 0;
        _blackboard.ConsecutiveChargeIsActive = true;
    }

    public override void ExitState()
    {

    }

    public override void SwitchHandling()
    {
        if (!_blackboard.ConsecutiveChargeIsActive)
        {
            parent.machine.SetState(_blackboard.stateFactory.Approach(), this.parent);
        }
    }

    //bool chargeIsRunning = false;
    //IEnumerator Charge()
    //{
    //    chargeIsRunning = true;
    //    // PERFORM CHARGES
    //    for (int chargeNum = 1; chargeNum <= sm.ConsecutiveCharges; chargeNum++)
    //    {
    //        yield return new WaitUntil(() => !sm.inflictionHandler.IsAfflicted(InflictionType.GREASY_Knockback));
    //        Vector2 vel = (sm.PlayerTransform.position - sm.transform.position).normalized * sm.ChargeForce * sm.GetMoveSpeed();
    //        for (float chargeTime = 0; chargeTime < sm.ChargeTime; chargeTime += Time.deltaTime)
    //        {
    //            if (sm._rigidbody.velocity.magnitude < sm.ChargeSpeed) sm._rigidbody.AddForce(vel * Time.deltaTime);
    //            yield return null;
    //        }

    //        if (chargeNum < sm.ConsecutiveCharges) yield return new WaitForSeconds(Random.Range(sm.ChargeCooldownTime.x, sm.ChargeCooldownTime.y));
    //        else yield return new WaitForSeconds(sm.FinalChargeCooldownTime);
    //    }
    //    chargeIsRunning = false;
    //}
}
