using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient.InflictionFlavor;

public class DetectionState : BaseState
{

    FlettuceAI _blackboard;
    IEnumerator IHandleCharge;
    public DetectionState(FlettuceAI blackboaord)
    {
        _blackboard = blackboaord;
    }
    public override void EnterState()
    {
        // Set The Attack Animation
        _blackboard.animator.Play("Attack");

        // Set the NavMeshAgent Speed to New Speed and Unstop Agent
        _blackboard.Agent.speed = _blackboard.GetMoveSpeed() * _blackboard.AttackSpeedMultiplier;
        _blackboard.Agent.isStopped = false;

        // Set the initial state to Idle
        machine.SetState(_blackboard.stateFactory.Approach(), this);

    }

    public override void ExitState()
    {
        
    }

    public override void DoActions()
    {
    }

    IEnumerator HandleCharge()
    {
        _blackboard.animator.Play("Ready");
        while (true)
        {
            
            float dist = 0;
            do
            {
                _blackboard.Agent.SetDestination(_blackboard.PlayerTransform.position);
                yield return new WaitForSeconds(_blackboard.AttackDistanceCheckInterval);
                dist = Vector2.Distance(_blackboard.transform.position, _blackboard.PlayerTransform.position);

                if (dist > _blackboard.DistanceFromPlayerToDisengage)
                {
                    _blackboard.stateMachine.SetState(_blackboard.stateFactory.Idle(), this.parent); // disengage if too far
                }

            } while (dist > _blackboard.DistanceToPlayerForCharge);
            _blackboard.Agent.isStopped = true;

            // PERFORM CHARGES
            _blackboard.animator.Play("Attack");
            for (int chargeNum = 1; chargeNum <= _blackboard.NumConsecutiveCharges; chargeNum++)
            {
                yield return new WaitUntil(() => !_blackboard.inflictionHandler.IsAfflicted(InflictionType.GREASY_Knockback));
                Vector2 vel = (_blackboard.PlayerTransform.position - _blackboard.transform.position).normalized * _blackboard.ChargeForce * _blackboard.GetMoveSpeed();
                for (float chargeTime = 0; chargeTime < _blackboard.ChargeTime; chargeTime += Time.deltaTime)
                {
                    if (_blackboard._rigidbody.velocity.magnitude < _blackboard.ChargeSpeed) _blackboard._rigidbody.AddForce(vel * Time.deltaTime);
                    yield return null;
                }

                if (chargeNum < _blackboard.NumConsecutiveCharges) yield return new WaitForSeconds(Random.Range(_blackboard.ChargeCooldownTime.x, _blackboard.ChargeCooldownTime.y));
                else yield return new WaitForSeconds(_blackboard.FinalChargeCooldownTime);
            }
        }
    }

    public override void SwitchHandling()
    {
        // Check for events
        if (_blackboard.Events.EnemyOutOfChaseEvent())
        {
            parent.machine.SetState(_blackboard.stateFactory.Idle(), this.parent); // disengage if too far
        }
    }
}
