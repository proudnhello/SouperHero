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
        return new IdleState(_blackboard);
    }

    public BaseState Attack()
    {
        return new DetectionState(_blackboard);
    }

    public BaseState Approach()
    {
        return new ApproachState(_blackboard);
    }

    public BaseState Charge()
    {
        return new ChargeState(_blackboard);
    }


}
