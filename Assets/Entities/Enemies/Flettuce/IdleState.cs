using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
    }

    public override void ExitState()
    {

    }

    public override void DoActions()
    {
        centerPoint = _blackboard.transform.position;
        HandleDetection();

        //HandlePatrol();
    }

    // Detection Global Variables
    float timeAtFreeze = -1f;

    public void HandleDetection()
    {

        _blackboard.Agent.speed = _blackboard.GetMoveSpeed() * _blackboard.IdleSpeedMultiplier;

        // Check if enemy should be frozen
        if (_blackboard.freezeEnemy)
        {
            if (timeAtFreeze == -1f)
            {
                timeAtFreeze = Time.time;
            }
            else
            {
                if(Mathf.Abs(Time.time - timeAtFreeze) > _blackboard.PlayerDetectionIntervalWhenFrozen)
                {
                    Debug.Log("Enemy is frozen");
                    return;
                }
            }
        }
    }

    public void HandlePatrol()
    {

        // Check if enemy should be frozen
        if (_blackboard.freezeEnemy)
        {
            if (timeAtFreeze == -1f)
            {
                timeAtFreeze = Time.time;
            }
            else
            {
                if (Mathf.Abs(Time.time - timeAtFreeze) > _blackboard.PlayerDetectionIntervalWhenFrozen)
                {
                    return;
                }
            }
        }

        //// SIT THERE FOR A BIT
        //yield return new WaitForSeconds(Random.Range(sm.PatrolWaitTime.x, sm.PatrolWaitTime.y));

        // FIND NEW POINT
        float targetAngle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        Vector2 targetDir = new Vector3(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
        Vector2 targetPoint = targetDir * UnityEngine.Random.Range(_blackboard.PatrolDistance.x, _blackboard.PatrolDistance.y) + centerPoint;

        _blackboard.Agent.isStopped = false;
        _blackboard.Agent.SetDestination(targetPoint);

        Vector2 lastPos;
        do
        {
            lastPos = _blackboard.Agent.transform.position;
            
            
            //yield return new WaitForSeconds(sm.WhilePatrolCheckIfStoppedInterval);


            // check that the Agent is moving far enough every interval to ensure it's not blocked
        } while (Vector2.Distance(lastPos, _blackboard.Agent.transform.position) > _blackboard.WhilePatrolCheckIfStoppedDistance &&
        Vector2.Distance(targetPoint, _blackboard.Agent.transform.position) > .2f);
        _blackboard.Agent.isStopped = true;
    }

    public override void SwitchHandling()
    {
        if (_blackboard.Events.PlayerInDetectionRangeEvent())
        {
            Debug.Log(parent is null);
            Debug.Log(parent.machine is null);
            Debug.Log(this.parent is null);
            parent.machine.SetState(_blackboard.stateFactory.Detection(), this.parent);

            Debug.Log("Player in detection range");
        }
    }
}