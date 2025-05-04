using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{

    public BaseState currentState;
    public BaseState CurrentState { get { return currentState; } }

    // Sets the state to newState
    public void SetState(BaseState newState, BaseState parentState, bool forceReset = false)
    {
        if (currentState != newState || forceReset)
        {
            currentState?.ExitStateBranch();
            parentState.SetSubState(newState);
            currentState = newState;
            currentState.EnterState();
        }
    }

    // Sets the state to randomly to one of the states in the list
    // https://docs.unity3d.com/6000.0/Documentation/Manual/class-random.html
    public void SetStateRandom(List <BaseState> newStates, BaseState parentState)
    {
        SetState(newStates[Random.Range(0, newStates.Count)], parentState);
    }

}
