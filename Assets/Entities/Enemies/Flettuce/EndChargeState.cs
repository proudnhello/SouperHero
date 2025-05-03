using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndChargeState : BaseState
{
    FlettuceAI _blackboard;
    public EndChargeState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
    }

    public override void DoActions()
    {
        
    }

    public override void EnterState()
    {
        _blackboard.ConsecutiveChargeIsActive = false;
    }

    public override void ExitState()
    {
        
    }


    // End of State Don't Need To Switch
    public override void SwitchHandling()
    {
        
    }

}
