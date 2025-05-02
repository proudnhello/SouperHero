// This is separate from the status effect frozen, this is for freezing enemy movement off screen to save computation

using UnityEngine;

public class FrozenState : BaseState
{
    FlettuceAI _blackboard;

    // Distance Check Global Variables
    float _timeAtFreeze = -1f;
    public FrozenState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
    }
    public override void EnterState()
    {
        _blackboard.animator.enabled = false;
        _blackboard.Agent.isStopped = true;
        _timeAtFreeze = Time.time;
    }

    public override void ExitState()
    {
        _blackboard.animator.enabled = true;
        _blackboard.Agent.isStopped = false;
    }

    public override void DoActions()
    {

    }

    public override void SwitchHandling()
    {
        if (Mathf.Abs(Time.time - _timeAtFreeze) > _blackboard.PlayerDetectionIntervalWhenFrozen)
        {
            if (!_blackboard.Events.PlayerOutOfFrozenRangeEvent())
            {
                parent.machine.SetState(_blackboard.stateFactory.Idle(), this.parent);
            }
            else
            {
                _timeAtFreeze = Time.time;
            }
        }
    }
}