using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using static FlavorIngredient.InflictionFlavor;

public class WaitState : BaseState
{
    FlettuceAI _blackboard;
    float _startTime;
    float _waitTime;
    BaseState _switchToAfterState;
    public WaitState(FlettuceAI blackboard, float waitTime, BaseState switchToAfterState)
    {
        _blackboard = blackboard;
        _waitTime = waitTime;
        _switchToAfterState = switchToAfterState;
    }

    public override void DoActions()
    {
        
    }

    public override void EnterState()
    {
        _startTime = Time.time;
    }

    public override void ExitState()
    {
        
    }

    public override void SwitchHandling()
    {
        if (_waitTime < Mathf.Abs(_startTime - Time.time))
        {
            parent.machine.SetState(_switchToAfterState, this.parent);
        }
    }
}
