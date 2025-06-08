using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerAI : EnemyBaseClass
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
    [SerializeField] protected float MaxPatrolPathLength;
    [SerializeField] protected Vector2 PatrolWaitTime = new Vector2(1.5f, 2.5f);
    [SerializeField] protected float WhilePatrolCheckIfStoppedInterval = .3f;
    [SerializeField] protected float WhilePatrolCheckIfStoppedDistance = .1f;
    [SerializeField] protected float IdleSpeedMultiplier = 1f;

    [Header("Attack State")]
    [SerializeField] protected float AttackSpeedMultiplier = 1.5f;
    [SerializeField] protected float AttackDistanceCheckInterval = .3f;
    [SerializeField] protected float DistanceFromPlayerToDisengage = 20f;
    // [SerializeField] protected float SlipperySurfaceMultiplier = 1.15f;
    [SerializeField] protected float MinimumTimeBetweenCharges = 2f;
    [SerializeField] protected float DistanceToPlayerForCharge = 5f;
    [SerializeField] protected float ChargeSpeed = 5f;
    [SerializeField] protected float ChargeForce = 100f;
    [SerializeField] protected float ChargeTime = 1f;
        
    protected Animator animator;
    internal List<IState> states;
    internal IState currentState;
    bool freezeEnemy = false;
    void Start()
    {
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
    protected override void Update()
    {
        if (IsDead()) return;
        freezeEnemy = Vector2.Distance (transform.position, PlayerEntityManager.Singleton.transform.position) > FreezeEnemyWhenThisFar;
    }

    protected void Death()
    {
        currentState.OnExit();
        agent.updatePosition = false;
        SetHealth(0);
        Destroy(gameObject);
    }

    public class IdleState : IState
    {
        DeerAI sm;
        IEnumerator IHandleDetection;
        IEnumerator IHandlePatrol;
        Vector2 centerPoint;
        UnityEngine.AI.NavMeshPath path;
        public IdleState(DeerAI _sm)
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

        float CalculatePathLength()
        {
            float distance = -1;
            if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete) return distance;

            distance = Vector2.Distance(sm.transform.position, path.corners[0]);
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector2.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
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

                UnityEngine.AI.NavMesh.CalculatePath(new Vector2(sm.transform.position.x, sm.transform.position.y), sm._playerTransform.position,
                    UnityEngine.AI.NavMesh.AllAreas, path);

                float distance = CalculatePathLength();
                if ((distance >= 0 && distance < sm.PlayerDetectionPathLength) || sm.alwaysAggro)
                {
                    // if player is within certain distance, start attacking
                    sm.ChangeState(ChargerStates.ATTACK);
                }
                yield return new WaitForSeconds(sm.PlayerDetectionInterval);
            }
        }

        IEnumerator HandlePatrol()
        {
            bool towardsCenter = false;
            while (true)
            {
                sm.agent.speed = sm.GetMoveSpeed() * sm.IdleSpeedMultiplier;
                sm.animator.Play("Idle");
                if (sm.freezeEnemy)
                {
                    yield return new WaitForSeconds(sm.PlayerDetectionIntervalWhenFrozen);
                    continue;
                }

                // SIT THERE FOR A BIT
                yield return new WaitForSeconds(Random.Range(sm.PatrolWaitTime.x, sm.PatrolWaitTime.y));

                // FIND NEW POINT
                Vector2 targetPoint;
                if (towardsCenter)
                {
                    targetPoint = centerPoint;
                } else
                {
                    float distance;
                    do
                    {                        
                        float targetAngle = Random.Range(0, 2 * Mathf.PI);
                        Vector2 targetDir = new Vector3(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
                        float dist = Random.Range(sm.PatrolDistance.x, sm.PatrolDistance.y);
                        targetPoint = targetDir * Random.Range(sm.PatrolDistance.x, sm.PatrolDistance.y) + centerPoint;
                        UnityEngine.AI.NavMesh.CalculatePath(new Vector2(sm.transform.position.x, sm.transform.position.y), targetPoint,
                            UnityEngine.AI.NavMesh.AllAreas, path);
                        distance = CalculatePathLength();
                    } while (distance < 0 || distance >= sm.MaxPatrolPathLength);
                    
                    
                }
                
                sm.agent.isStopped = false;
                sm.agent.SetDestination(targetPoint);
                // sm.animator.Play("Walk");
                sm._sprite.flipX = sm.agent.path.corners[0].x > sm.transform.position.x;
                Vector2 lastPos;
                do
                {
                    lastPos = sm.agent.transform.position;
                    sm._sprite.flipX = sm.agent.velocity.x > 0;
                    yield return new WaitForSeconds(sm.WhilePatrolCheckIfStoppedInterval);
                    // check that the agent is moving far enough every interval to ensure it's not blocked
                } while (Vector2.Distance(lastPos, sm.agent.transform.position) > sm.WhilePatrolCheckIfStoppedDistance &&
                            Vector2.Distance(targetPoint, sm.agent.transform.position) > .2f);
                sm.agent.isStopped = true;
                towardsCenter = !towardsCenter;
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
        DeerAI sm;
        IEnumerator IHandleCharge;
        public AttackState(DeerAI _sm)
        {
            sm = _sm;
        }
        public void OnEnter()
        {
            sm.StartCoroutine(IHandleCharge = HandleCharge());

        }

        IEnumerator HandleCharge()
        {
            // sm.animator.Play("Walk");
            while (true)
            {              
                sm.agent.isStopped = false;
                float dist = 0;
                float time = 0;
                do
                {
                    sm.agent.SetDestination(sm._playerTransform.position);

                    // once tater's rb has slowed down enough, then allow sprite to flip based on nav agent navigation
                    if (sm.agent.velocity.magnitude > sm._rigidbody.velocity.magnitude) sm._sprite.flipX = sm.agent.destination.x > sm.transform.position.x;

                    yield return new WaitForSeconds(sm.AttackDistanceCheckInterval);
                    sm.agent.speed = sm.GetMoveSpeed() * sm.AttackSpeedMultiplier;
                    dist = Vector2.Distance(sm.transform.position, sm._playerTransform.position);

                    if (dist > sm.DistanceFromPlayerToDisengage && !sm.alwaysAggro)
                    {
                        sm.ChangeState(ChargerStates.IDLE); // disengage if too far (but not if set to always aggro)
                    }

                    time += sm.AttackDistanceCheckInterval;
                } while (time < sm.MinimumTimeBetweenCharges || dist > sm.DistanceToPlayerForCharge || !sm.CanAttack());
                sm.agent.isStopped = true;

                // PERFORM CHARGES if can attack
                // sm.animator.Play("Attack");
                Vector2 vel = (sm._playerTransform.position - sm.transform.position).normalized * sm.ChargeForce * sm.GetMoveSpeed();
                sm._sprite.flipX = vel.x > 0;
                for (float chargeTime = 0; chargeTime < sm.ChargeTime; chargeTime += Time.deltaTime)
                {
                    if (sm._rigidbody.velocity.magnitude < sm.ChargeSpeed) sm._rigidbody.AddForce(vel * Time.deltaTime);
                    yield return null;
                }

                // sm.animator.Play("Walk");

            }
        }

        public void OnExit()
        {
            if (IHandleCharge != null) sm.StopCoroutine(IHandleCharge);
        }
    }

}
