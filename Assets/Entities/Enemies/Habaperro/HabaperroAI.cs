using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Entity;

public class HabaperroAI : EnemyBaseClass
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
    [SerializeField] protected float DistanceToPlayerForExplosion = 5f;
    [SerializeField] protected float AttackDistanceCheckInterval = .3f;
    [SerializeField] protected float DistanceFromPlayerToDisengage = 20f;
    [SerializeField] protected float TimeForIgniteAnim = 1f;
    [SerializeField] protected Color IgnitionFlashColor;
    [SerializeField] protected float IgnitionFlashStartInterval;
    [SerializeField] protected float IgnitionFlashIntervalMultiplier;
    [SerializeField] protected float IgnitionFlashIntervalToTriggerExplosion;
    [SerializeField] protected float PostExplosionWaitTime = 1f;
    [SerializeField] private Explosion explosion;
    [SerializeField] protected Vector3 ExplosionSpawnOffset;

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
        freezeEnemy = Vector2.Distance(transform.position, PlayerEntityManager.Singleton.transform.position) > FreezeEnemyWhenThisFar;
    }

    protected override void Die()
    {
        currentState.OnExit();
        agent.updatePosition = false;
        SetHealth(0);
        Destroy(gameObject);
    }

    public class IdleState : IState
    {
        HabaperroAI sm;
        IEnumerator IHandleDetection;
        IEnumerator IHandlePatrol;
        Vector2 centerPoint;
        NavMeshPath path;
        public IdleState(HabaperroAI _sm)
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
            if (path.status != NavMeshPathStatus.PathComplete) return distance;

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

                NavMesh.CalculatePath(new Vector2(sm.transform.position.x, sm.transform.position.y), sm._playerTransform.position,
                    NavMesh.AllAreas, path);

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
                        NavMesh.CalculatePath(new Vector2(sm.transform.position.x, sm.transform.position.y), targetPoint,
                            NavMesh.AllAreas, path);
                        distance = CalculatePathLength();
                        //Debug.Log("target point = " + targetPoint + " " + dist + "m away, " + distance + "steps");
                    } while (distance < 0 || distance >= sm.MaxPatrolPathLength);
                    
                    
                }
                
                sm.agent.isStopped = false;
                sm.agent.SetDestination(targetPoint);
                sm.animator.Play("Walk");
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
        HabaperroAI sm;
        IEnumerator IHandleMovementExplosion;
        public AttackState(HabaperroAI _sm)
        {
            sm = _sm;
        }
        public void OnEnter()
        {
            sm.agent.speed = sm.GetMoveSpeed() * sm.AttackSpeedMultiplier;
            sm.StartCoroutine(IHandleMovementExplosion = HandleMovementExplosion());
        }

        IEnumerator HandleMovementExplosion()
        {            
            sm.animator.Play("Walk");
            sm.agent.isStopped = false;
            float dist = 0;
            do
            {
                sm.agent.speed = sm.GetMoveSpeed() * sm.IdleSpeedMultiplier;
                sm.agent.SetDestination(sm._playerTransform.position);
                sm._sprite.flipX = sm.agent.velocity.x > 0;
                yield return new WaitForSeconds(sm.AttackDistanceCheckInterval);
                dist = Vector2.Distance(sm.transform.position, sm._playerTransform.position);

                if (dist > sm.DistanceFromPlayerToDisengage && !sm.alwaysAggro)
                {
                    sm.ChangeState(ChargerStates.IDLE); // disengage if too far
                }

            } while (dist > sm.DistanceToPlayerForExplosion);
            sm.agent.isStopped = true;

            // EXPLODE
            sm.animator.Play("Ignite");

            float interval = sm.IgnitionFlashStartInterval;
            do
            {
                sm._sprite.color = Color.white;
                yield return new WaitForSeconds(interval);
                sm._sprite.color = sm.IgnitionFlashColor;
                yield return new WaitForSeconds(interval);
                    
                interval *= sm.IgnitionFlashIntervalMultiplier;
            } while (interval > sm.IgnitionFlashIntervalToTriggerExplosion);
            Debug.Log("Got here1");
            sm._sprite.color = Color.white;
            sm._collider.enabled = false;
            sm.animator.Play("Boom");
            Debug.Log("Got here2");
            Instantiate(sm.explosion, sm.transform.position + sm.ExplosionSpawnOffset, Quaternion.identity);
            Debug.Log("Got here3");
            yield return new WaitForSeconds(sm.PostExplosionWaitTime);
            Debug.Log("Got here4");
            IHandleMovementExplosion = null;
            sm.Die();
        }

        public void OnExit()
        {
            if (IHandleMovementExplosion != null) sm.StopCoroutine(IHandleMovementExplosion);
            sm._sprite.color = Color.white;
        }
    }

}
