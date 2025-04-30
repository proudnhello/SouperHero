using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

public abstract class BaseState : IState
{
    public BaseState parent;
    public List<BaseState> children;
    public StateMachine machine;
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void DoActions();
    public abstract void SwitchHandling();

    // Constructor
    public BaseState()
    {
        machine = new();
        children = new List<BaseState>();
    }

    // Next Step Calls DoActions() and SwitchHandling()
    public void NextStep()
    {
        DoActionsBranch();
        SwitchHandlingBranch();
    }

    // Recursively does actions of internal state machine
    public void DoActionsBranch()
    {
        DoActions();
        machine.currentState?.DoActionsBranch();
    }

    public void SwitchHandlingBranch()
    {
        SwitchHandling();
        machine.currentState?.SwitchHandlingBranch();
    }

    // Recursively does exits internal state machine
    public void ExitStateBranch()
    {
        ExitState();
        machine.currentState?.ExitStateBranch();
    }

    public void SetSuperState(BaseState newSuperState)
    {
        parent = newSuperState;
    }

    public void SetSubState(BaseState newSubState)
    {
        children.Append(newSubState);
        SetSuperState(this);
    }

}
