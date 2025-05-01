using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEditor.Rendering;
using UnityEngine;

public class RootFlettuceState : BaseState
{

    public enum PossibleSubStates
    {
        IDLE,
        ATTACK
    }

    FlettuceAI _blackboard;
    public RootFlettuceState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
    }

    public override void EnterState() { 
        machine.SetState(_blackboard.stateFactory.Idle(), this);
    }
    public override void ExitState() { }

    public override void DoActions() {
        Debug.Log("The current state in root state is: " + machine.currentState.ToString());
    }

    // The Root State Has No Sibling States So Doesn't Need Switch Handling
    public override void SwitchHandling()
    {
        return;
    }
}
