using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static FlavorIngredient.InflictionFlavor;

//The ranged enemy follows player constantly until they are in range, at which point they freeze and fire bullets
//Code tutorial for ranged enemy found at: https://www.youtube.com/watch?v=bwi4lteomic

public class HopShroomAI : EnemyBaseClass
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
    [SerializeField] Transform firingPoint;
    [SerializeField] HopShroomSpore bullet;

    [Header("Idle State")]
    [SerializeField] protected float PlayerDetectionPathLength = 15f;
    [SerializeField] protected float PlayerDetectionInterval = .5f;
    [SerializeField] protected float IdleSpeedMultiplier = 1f;

    [Header("Attack State")]
    [SerializeField] protected float AttackSpeedMultiplier = 1.5f;
    [SerializeField] protected float MidDistanceShootPoint = 7;
    [SerializeField] protected Vector2 DistanceRangeToPlayerForShoot = new Vector2(5, 8);
    [SerializeField] protected float AttackDistanceCheckInterval = .3f;
    [SerializeField] protected float AttackMoveCheckMinimumDistance = .2f;
    [SerializeField] protected float DistanceFromPlayerToDisengage = 20f;
    [SerializeField] private float TimeToStartShooting = 0.5f;
    [SerializeField] private float ShotAnimDelay = .2f;
    [SerializeField] private int ShotCount = 3;
    [SerializeField] private float ShotCooldown = 2.0f;
    [SerializeField] private float FinalShotCooldown = 0.3f;

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
        base.Die();
    }

    public class IdleState : IState
    {
        HopShroomAI sm;
        IEnumerator IHandleDetection;
        NavMeshPath path;
        public IdleState(HopShroomAI _sm)
        {
            sm = _sm;
            path = new();
        }
        public void OnEnter()
        {
            sm.animator.Play("Idle");
            sm.agent.speed = sm.GetMoveSpeed() * sm.IdleSpeedMultiplier;
            sm.StartCoroutine(IHandleDetection = HandleDetection());
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
                if (sm.freezeEnemy)
                {
                    yield return new WaitForSeconds(sm.PlayerDetectionIntervalWhenFrozen);
                    continue;
                }

                NavMesh.CalculatePath(new Vector2(sm.transform.position.x, sm.transform.position.y), sm._playerTransform.position,
                    NavMesh.AllAreas, path);

                float distance = CalculatePathLength();
                if (distance >= 0 && distance < sm.PlayerDetectionPathLength)
                {
                    // if player is within certain distance, start attacking
                    sm.ChangeState(ChargerStates.ATTACK);
                }
                yield return new WaitForSeconds(sm.PlayerDetectionInterval);
            }
        }

        public void OnExit()
        {
            if (IHandleDetection != null) sm.StopCoroutine(IHandleDetection);
        }
    }

    public class AttackState : IState
    {
        HopShroomAI sm;
        IEnumerator IHandleShoot;
        public AttackState(HopShroomAI _sm)
        {
            sm = _sm;
        }
        public void OnEnter()
        {
            sm.agent.speed = sm.GetMoveSpeed() * sm.AttackSpeedMultiplier;
            sm.StartCoroutine(IHandleShoot = HandleShoot());
        }

        IEnumerator HandleShoot()
        {
            while (true)
            {
                sm.animator.Play("Walk");
                sm.agent.isStopped = false;
                float dist = 0;
                Vector2 lastPos;
                do
                {
                    Vector3 offset = (sm.transform.position - sm._playerTransform.position).normalized * sm.MidDistanceShootPoint;
                    sm.agent.SetDestination(sm._playerTransform.position + offset);
                    sm._sprite.flipX = sm.agent.destination.x <= sm.transform.position.x;
                    lastPos = sm.agent.transform.position;
                    yield return new WaitForSeconds(sm.AttackDistanceCheckInterval);
                    dist = Vector2.Distance(sm.transform.position, sm._playerTransform.position);

                    if (dist > sm.DistanceFromPlayerToDisengage)
                    {
                        sm.ChangeState(ChargerStates.IDLE); // disengage if too far
                    }
                } while ((dist < sm.DistanceRangeToPlayerForShoot.x && 
                            Vector2.Distance(lastPos, sm.agent.transform.position) > sm.AttackMoveCheckMinimumDistance)
                            || dist > sm.DistanceRangeToPlayerForShoot.y);
                sm.agent.isStopped = true;

                // EXPLODE
                sm.animator.Play("Idle");
                yield return new WaitForSeconds(sm.TimeToStartShooting);

                // PERFORM CHARGES
                for (int chargeNum = 1; chargeNum <= sm.ShotCount; chargeNum++)
                {
                    sm.animator.Play("Attack");
                    yield return new WaitForSeconds(sm.ShotAnimDelay);
                    HopShroomSpore temp = Instantiate(sm.bullet, sm.firingPoint.position, sm.firingPoint.rotation);
                    temp.direction = (sm._playerTransform.position - sm.transform.position).normalized;
                    sm._sprite.flipX = temp.direction.x <= 0;
                    if (chargeNum < sm.ShotCount) yield return new WaitForSeconds(sm.ShotCooldown);
                    else yield return new WaitForSeconds(sm.FinalShotCooldown);
                }
            }       
        }

        public void OnExit()
        {
            if (IHandleShoot != null) sm.StopCoroutine(IHandleShoot);
        }
    }
}
