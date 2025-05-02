using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : BaseState
{
    FlettuceAI _blackboard;
    IEnumerator IHandleDetection;
    IEnumerator IHandlePatrol;
    Vector2 centerPoint;
    RootFlettuceState.PossibleSubStates currentStates;
    NavMeshPath path;
    public IdleState(FlettuceAI blackboard)
    {
        _blackboard = blackboard;
        path = new();
    }
    public override void EnterState()
    {
        Debug.Log("Enter Idle State");
        _blackboard.animator.Play("Idle");
        _blackboard.Agent.speed = _blackboard.GetMoveSpeed() * _blackboard.IdleSpeedMultiplier;
    }

    public override void ExitState()
    {

    }

    public override void DoActions()
    {
        centerPoint = _blackboard.transform.position;

        HandlePatrol();

        Debug.Log($"The current timer is {currentTimer}");
    }

    Vector2 lastPos;
    int currentTimer = -1;
    Vector2 targetPoint;

    public void HandlePatrol()
    {

        //// SIT THERE FOR A BIT
        //yield return new WaitForSeconds(Random.Range(sm.PatrolWaitTime.x, sm.PatrolWaitTime.y));

        if (currentTimer == -1)
        {
            if (!_blackboard.HasTimerStarted())
            {
                _blackboard.StartWaitTimer(Random.Range(_blackboard.PatrolWaitTime.x, _blackboard.PatrolWaitTime.y));
            }
            else
            {
                if (!_blackboard.CheckWaitTimer())
                {
                    return;
                }
                else
                {
                    // SET NEW DESTINATION WHEN INITIAL TIMER IS UP
                    float targetAngle = Random.Range(0, 2 * Mathf.PI);
                    Vector2 targetDir = new Vector3(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
                    targetPoint = targetDir * Random.Range(_blackboard.PatrolDistance.x, _blackboard.PatrolDistance.y) + centerPoint;

                    _blackboard.Agent.isStopped = false;
                    _blackboard.Agent.SetDestination(targetPoint);

                    // Increment What the Timer is
                    currentTimer = currentTimer + 1;
                }
            }
        }


        // Wait Timer For Distance Checking
        if (currentTimer >= 0)
        {
            if (!_blackboard.HasTimerStarted())
            {
                _blackboard.StartWaitTimer(Random.Range(_blackboard.PatrolWaitTime.x, _blackboard.PatrolWaitTime.y));
            }
            else
            {
                if (!_blackboard.CheckWaitTimer())
                {
                    return;
                } else
                {
                    // CHeck Distance When Time Is Up
                    if (Vector2.Distance(lastPos, _blackboard.Agent.transform.position) > _blackboard.WhilePatrolCheckIfStoppedDistance && Vector2.Distance(targetPoint, _blackboard.Agent.transform.position) > .2f)
                    {
                        lastPos = _blackboard.Agent.transform.position;
                        return;
                    }
                    else
                    {
                        // If not moved set agent to start 
                        _blackboard.Agent.isStopped = true;
                        currentTimer = -1;
                    }
                }
            }
        }
    }

    public override void SwitchHandling()
    {
        if (_blackboard.Events.PlayerInDetectionRangeEvent())
        {
            parent.machine.SetState(_blackboard.stateFactory.Detection(), this.parent);

            Debug.Log("Player in detection range");
        } else if (_blackboard.Events.PlayerOutOfFrozenRangeEvent())
        {
            parent.machine.SetState(_blackboard.stateFactory.Frozen(), this.parent);
        }

       
    }
}