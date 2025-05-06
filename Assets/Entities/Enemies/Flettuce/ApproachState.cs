using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachState : BaseState
{

    FlettuceAI _blackboard;
    public ApproachState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
    }

    public override void DoActions()
    {
        Approach();
    }

    public override void EnterState()
    {
        _blackboard.animator.Play("Ready");
    }

    public override void ExitState()
    {
        _blackboard.Agent.isStopped = true;
    }

    public void Approach()
    {
        _blackboard.Agent.isStopped = false;
        _blackboard.Agent.SetDestination(_blackboard.PlayerTransform.position);
    }

    public override void SwitchHandling()
    {
        if (_blackboard.Events.PlayerInChargeRangeEvent())
        {
            parent.machine.SetState(_blackboard.stateFactory.ConsecutiveCharge(), this.parent);
        }
    }

    //IEnumerator Approach()
    //{
    //    float dist = 0;
    //    sm.animator.Play("Ready");
    //    do
    //    {
    //        sm.Agent.SetDestination(sm.PlayerTransform.position);
    //        yield return new WaitForSeconds(sm.AttackDistanceCheckInterval);
    //        dist = Vector2.Distance(sm.transform.position, sm.PlayerTransform.position);

    //        if (dist > sm.DistanceFromPlayerToDisengage)
    //        {
    //            sm.stateMachine.SetState(sm.stateFactory.Idle()); // disengage if too far
    //        }

    //    } while (dist > sm.DistanceToPlayerForCharge);
    //    sm.Agent.isStopped = true;
    //}
}
