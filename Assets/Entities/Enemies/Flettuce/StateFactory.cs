using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateFactory
{

    //StateMachine _machine;
    FlettuceAI _blackboard;

    public StateFactory(FlettuceAI blackboard, StateMachine machine)
    {
        //_machine = machine;
        _blackboard = blackboard;
    }

    public BaseState RootFlettuceState()
    {
        return new RootFlettuceState(_blackboard);
    }

    public BaseState Idle()
    {
        return new FlettuceIdleState(_blackboard);
    }

    public BaseState Detection()
    {
        return new FlettuceDetectionState(_blackboard);
    }

    public BaseState Frozen()
    {
        return new FlettuceFrozenState(_blackboard);
    }

    public BaseState Approach()
    {
        return new FlettuceApproachState(_blackboard);
    }

    public BaseState ConsecutiveCharge()
    {
        return new FlettuceConsecutiveChargeState(_blackboard);
    }

    public BaseState Charge()
    {
        return new FlettuceChargeState(_blackboard);
    }

    public BaseState WaitState(float waitTime, BaseState switchToAfterState)
    {
        return new WaitState(_blackboard, waitTime, switchToAfterState);
    }

    public BaseState EndCharge()
    {
        return new EndChargeState(_blackboard);
    }

}
