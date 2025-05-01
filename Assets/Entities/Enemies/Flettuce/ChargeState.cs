using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using static FlavorIngredient.InflictionFlavor;

public class ChargeState : BaseState
{
    FlettuceAI _blackboard;
    int _chargeNum;
    float _chargeUpStartTime;
    bool _endChargeState;
    public ChargeState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
    }

    public override void DoActions()
    {
        //yield return new WaitUntil(() => !_blackboard.inflictionHandler.IsAfflicted(InflictionType.GREASY_Knockback));


        Vector2 vel = (_blackboard.PlayerTransform.position - _blackboard.transform.position).normalized * _blackboard.ChargeForce * _blackboard.GetMoveSpeed();
        if (_blackboard.ChargeTime > Mathf.Abs(_chargeUpStartTime - Time.time)) {
            if (_blackboard._rigidbody.velocity.magnitude < _blackboard.ChargeSpeed)
            {
                _blackboard._rigidbody.AddForce(vel * Time.deltaTime);
            }
            return;
        }

        _endChargeState=true;


        //if (chargeNum < _blackboard.ConsecutiveCharges) yield return new WaitForSeconds(Random.Range(_blackboard.ChargeCooldownTime.x, _blackboard.ChargeCooldownTime.y));
        //else yield return new WaitForSeconds(_blackboard.FinalChargeCooldownTime);
    }

    public override void EnterState()
    {
        _blackboard.animator.Play("Attack");
        _chargeUpStartTime = Time.time;
        _endChargeState  = false;
    }

    public override void ExitState()
    {
        _blackboard.ChargesCounter++;
    }

    public override void SwitchHandling()
    {
        if (_endChargeState)
        {
            if(_blackboard.ChargesCounter < _blackboard.NumConsecutiveCharges)
            {
                float waitTime = Random.Range(_blackboard.ChargeCooldownTime.x, _blackboard.ChargeCooldownTime.y);
                parent.machine.SetState(_blackboard.stateFactory.WaitState(waitTime, _blackboard.stateFactory.Charge()), this.parent);
            } else
            {
                float waitTime = _blackboard.FinalChargeCooldownTime;
                parent.machine.SetState(_blackboard.stateFactory.WaitState(waitTime, _blackboard.stateFactory.EndCharge()), this.parent);
            }
            
        }
    }
}
