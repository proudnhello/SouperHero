using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void DoActions();
    public abstract void DoActionsBranch();

}
