using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

public class FlettuceAI : EnemyBaseClass
{

    // ~~~ DEFINITIONS ~~~
    public enum ChargerStates
    {
        IDLE,
        ATTACK
    }
    public interface IState
    {
        public void OnEnter();
        public void OnExit();
    }

    // ~~~ VARIABLES ~~~

    [Header("General")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float FreezeEnemyWhenThisFar = 30f;
    [SerializeField] protected float PlayerDetectionIntervalWhenFrozen = 1.5f;

    [Header("Idle State")]
    [SerializeField] protected float PlayerDetectionPathLength = 15f;
    [SerializeField] protected float PlayerDetectionInterval = .5f;
    [SerializeField] protected Vector2 PatrolDistance = new Vector2(2.5f, 3.5f);
    [SerializeField] protected Vector2 PatrolWaitTime = new Vector2(1.5f, 2.5f);
    [SerializeField] protected float WhilePatrolCheckIfStoppedInterval = .3f;
    [SerializeField] protected float WhilePatrolCheckIfStoppedDistance = .1f;
    [SerializeField] protected float IdleSpeedMultiplier = 1f;

    [Header("Attack State")]
    [SerializeField] protected float AttackSpeedMultiplier = 1.5f;
    [SerializeField] protected float DistanceToPlayerForCharge = 5f;
    [SerializeField] protected float AttackDistanceCheckInterval;
    [SerializeField] protected float ChargeSpeed = 5f;
    [SerializeField] protected float ChargeForce = 200f;
    [SerializeField] protected float ChargeTime = 1f;
    [SerializeField] protected int ConsecutiveCharges = 3;
    [SerializeField] protected Vector2 ChargeCooldownTime = new Vector2(.6f, .8f);
    [SerializeField] protected float FinalChargeCooldownTime = 1f;
    [SerializeField] protected float DistanceFromPlayerToDisengage = 20f;

    protected Animator animator;
    internal List<IState> states;
    internal IState currentState;
    bool freezeEnemy = false;

    public override void Fall(Transform _respawnPoint)
    {
        return; // flettuce fly lmao they're immune to pits
    }
    void Start(){
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        animator = GetComponent<Animator>();

        states = new()
        {
            new IdleState(this),
            new AttackState(this)
        };

        currentState = states[0];
        currentState.OnEnter();
    }

    public void ChangeState(ChargerStates state)
    {
        currentState.OnExit();
        currentState = states[(int)state];
        currentState.OnEnter();
    }
    protected override void Update(){
        if (IsDead()) return;
        freezeEnemy = Vector2.Distance(transform.position, PlayerEntityManager.Singleton.transform.position) > FreezeEnemyWhenThisFar;
        _sprite.flipX = agent.destination.x > transform.position.x || _rigidbody.velocity.x > 0;
    }

    protected override void Die()
    {
        currentState.OnExit();
        base.Die();
    }

    public class IdleState : IState
    {
        FlettuceAI sm;
        IEnumerator IHandleDetection;
        IEnumerator IHandlePatrol;
        Vector2 centerPoint;
        NavMeshPath path;
        public IdleState(FlettuceAI _sm)
        {
            sm = _sm;
            path = new();
        }
        public void OnEnter()
        {
            centerPoint = sm.transform.position;
            sm.StartCoroutine(IHandleDetection = HandleDetection());
            sm.StartCoroutine(IHandlePatrol = HandlePatrol());
        }

        IEnumerator HandleDetection()
        {
            while (true)
            {
                sm.agent.speed = sm.GetMoveSpeed() * sm.IdleSpeedMultiplier;
                if (sm.freezeEnemy)
                {
                    yield return new WaitForSeconds(sm.PlayerDetectionIntervalWhenFrozen);
                    continue;
                }

                NavMesh.CalculatePath(new Vector2(sm.transform.position.x, sm.transform.position.y), sm._playerTransform.position,
                    NavMesh.AllAreas, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    float distance = Vector2.Distance(sm.transform.position, path.corners[0]);
                    for (int i = 1; i < path.corners.Length; i++)
                    {
                        distance += Vector2.Distance(path.corners[i-1], path.corners[i]);
                    }
                    if (distance < sm.PlayerDetectionPathLength || sm.alwaysAggro)
                    {
                        // if player is within certain distance, start attacking
                        sm.ChangeState(ChargerStates.ATTACK);
                    }
                }
                yield return new WaitForSeconds(sm.PlayerDetectionInterval);
            }
        }

        IEnumerator HandlePatrol()
        {
            sm.animator.Play("Idle");
            while (true)
            {
                if (sm.freezeEnemy)
                {
                    yield return new WaitForSeconds(sm.PlayerDetectionIntervalWhenFrozen);
                    continue;
                }

                // SIT THERE FOR A BIT
                yield return new WaitForSeconds(Random.Range(sm.PatrolWaitTime.x, sm.PatrolWaitTime.y));

                // FIND NEW POINT
                float targetAngle = Random.Range(0, 2 * Mathf.PI);
                Vector2 targetDir = new Vector3(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
                Vector2 targetPoint = targetDir * Random.Range(sm.PatrolDistance.x, sm.PatrolDistance.y) + centerPoint;

                sm.agent.isStopped = false;
                sm.agent.SetDestination(targetPoint);

                Vector2 lastPos;
                do
                {
                    lastPos = sm.agent.transform.position;
                    yield return new WaitForSeconds(sm.WhilePatrolCheckIfStoppedInterval);
                    // check that the agent is moving far enough every interval to ensure it's not blocked
                } while (Vector2.Distance(lastPos, sm.agent.transform.position) > sm.WhilePatrolCheckIfStoppedDistance &&
                Vector2.Distance(targetPoint, sm.agent.transform.position) > .2f);
                sm.agent.isStopped = true;
            }
        }

        public void OnExit()
        {
            if (IHandleDetection != null) sm.StopCoroutine(IHandleDetection);
            if (IHandlePatrol != null) sm.StopCoroutine(IHandlePatrol);
        }

        
    }

    public class AttackState : IState
    {
        FlettuceAI sm;
        IEnumerator IHandleCharge;
        public AttackState(FlettuceAI _sm)
        {
            sm = _sm;
        }
        public void OnEnter()
        {
            sm.StartCoroutine(IHandleCharge = HandleCharge());
        }

        IEnumerator HandleCharge()
        {
            sm.animator.Play("Ready");
            while (true)
            { 
                sm.agent.speed = sm.GetMoveSpeed() * sm.AttackSpeedMultiplier;
                sm.agent.isStopped = false;
                float dist = 0;
                do
                {
                    sm.agent.SetDestination(sm._playerTransform.position);
                    yield return new WaitForSeconds(sm.AttackDistanceCheckInterval);
                    dist = Vector2.Distance(sm.transform.position, sm._playerTransform.position);

                    if (dist > sm.DistanceFromPlayerToDisengage && !sm.alwaysAggro)
                    {
                        sm.ChangeState(ChargerStates.IDLE); // disengage if too far
                    }

                } while (dist > sm.DistanceToPlayerForCharge);
                sm.agent.isStopped = true;

                // PERFORM CHARGES
                if (sm.CanAttack())
                {
                    sm.animator.Play("Attack");
                    for (int chargeNum = 1; chargeNum <= sm.ConsecutiveCharges; chargeNum++)
                    {
                        yield return new WaitUntil(() => !sm.inflictionHandler.IsAfflicted(InflictionType.GREASY_Knockback));
                        Vector2 vel = (sm._playerTransform.position - sm.transform.position).normalized * sm.ChargeForce * sm.GetMoveSpeed();
                        for (float chargeTime = 0; chargeTime < sm.ChargeTime; chargeTime += Time.deltaTime)
                        {
                            if (sm._rigidbody.velocity.magnitude < sm.ChargeSpeed) sm._rigidbody.AddForce(vel * Time.deltaTime);
                            yield return null;
                        }

                        if (chargeNum < sm.ConsecutiveCharges) yield return new WaitForSeconds(Random.Range(sm.ChargeCooldownTime.x, sm.ChargeCooldownTime.y));
                        else yield return new WaitForSeconds(sm.FinalChargeCooldownTime);
                    }
                }
            }
        }

        public void OnExit()
        {
            if (IHandleCharge != null) sm.StopCoroutine(IHandleCharge);
        }
    }

}
