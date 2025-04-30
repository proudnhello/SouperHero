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

    // ~~~ VARIABLES ~~~

    [Header("General")]
    [SerializeField] public LayerMask playerLayer;
    private float _freezeEnemyWhenThisFar = 30f;
    private float _playerDetectionIntervalWhenFrozen = 1.5f;

    // Idle State
    private float _playerDetectionPathLength = 15f;
    private float _playerDetectionInterval = .5f;
    private Vector2 _patrolDistance = new Vector2(2.5f, 3.5f);
    private Vector2 _patrolWaitTime = new Vector2(1.5f, 2.5f);
    private float _whilePatrolCheckIfStoppedInterval = .3f;
    private float _whilePatrolCheckIfStoppedDistance = .1f;
    private float _idleSpeedMultiplier = 1f;

    // Attack State
    private float _attackSpeedMultiplier = 1.5f;
    private float _distanceToPlayerForCharge = 5f;
    private float _attackDistanceCheckInterval;
    private float _chargeSpeed = 5f;
    private float _chargeForce = 200f;
    private float _chargeTime = 1f;
    private int _consecutiveCharges = 3;
    private Vector2 _chargeCooldownTime = new Vector2(.6f, .8f);
    private float _finalChargeCooldownTime = 1f;
    // NOTE: This should always be higher than PlayerDetectionPathLength
    private float _distanceFromPlayerToDisengage = 20f;

    // Getters and Setters
    public float FreezeEnemyWhenThisFar { get { return _freezeEnemyWhenThisFar; } }
    public float PlayerDetectionIntervalWhenFrozen { get { return _playerDetectionIntervalWhenFrozen; } }
    public float PlayerDetectionPathLength { get { return _freezeEnemyWhenThisFar; } }
    public float PlayerDetectionInterval { get { return _playerDetectionInterval; } }
    public Vector2 PatrolDistance { get { return _patrolDistance; } }
    public Vector2 PatrolWaitTime { get { return _patrolWaitTime; } }
    public float WhilePatrolCheckIfStoppedInterval { get { return _whilePatrolCheckIfStoppedInterval; } }
    public float WhilePatrolCheckIfStoppedDistance { get { return _whilePatrolCheckIfStoppedDistance; } }
    public float IdleSpeedMultiplier { get { return _idleSpeedMultiplier; } }
    public float AttackSpeedMultiplier { get {  return _attackSpeedMultiplier; } }
    public float DistanceToPlayerForCharge {  get {  return _distanceToPlayerForCharge; } }
    public float AttackDistanceCheckInterval {  get {  return _attackDistanceCheckInterval; } }
    public float ChargeSpeed {  get { return _chargeSpeed; } }
    public float ChargeForce {  get { return _chargeForce; } }
    public float ChargeTime {  get  { return _chargeTime; } }   
    public int ConsecutiveCharges {  get { return _consecutiveCharges; } }
    public Vector2 ChargeCooldownTime {  get { return _chargeCooldownTime; } }
    public float FinalChargeCooldownTime { get { return _finalChargeCooldownTime; } }
    public float DistanceFromPlayerToDisengage {  get { return _distanceFromPlayerToDisengage;  } }



    public Animator animator;
    internal List<IState> states;
    internal IState currentState;
    public bool freezeEnemy = false;

    public NavMeshAgent Agent { get { return agent; } }
    public Transform PlayerTransform { get { return _playerTransform; } }
    public bool AlwaysAggro { get { return alwaysAggro; } }

    public StateMachine stateMachine;

    public StateFactory stateFactory;

    public StateMachineEvents Events;

    NavMeshPath path;

    void Start(){
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        animator = GetComponent<Animator>();
        path = new();

        // Create Root State
        stateMachine = new();
        stateFactory = new(this, stateMachine);
        BaseState rootState = stateFactory.RootFlettuceState();
        stateMachine.SetState(rootState, rootState);

        // Make an Instance of the Class that defines Events
        Events = new(this);
    }

    protected override void UpdateAI(){

        // do actions
        stateMachine.CurrentState.NextStep();

        if (IsDead()) return;


        freezeEnemy = Vector2.Distance(transform.position, PlayerEntityManager.Singleton.transform.position) > FreezeEnemyWhenThisFar;
        _sprite.flipX = agent.destination.x > transform.position.x || _rigidbody.velocity.x > 0;
    }

    protected override void Die()
    {
        stateMachine.CurrentState.ExitState();
        base.Die();
    }

    public class StateMachineEvents
    {
        FlettuceAI _blackboard;
        public StateMachineEvents(FlettuceAI blackboard)
        {
            _blackboard = blackboard;
        }

        public bool EnemyOutOfChaseEvent()
        {
            // Calculate path from enemy to player
            NavMesh.CalculatePath(new Vector2(_blackboard.transform.position.x, _blackboard.transform.position.y), _blackboard._playerTransform.position,
                NavMesh.AllAreas, _blackboard.path);

            // Do this is the path exists
            if (_blackboard.path.status == NavMeshPathStatus.PathComplete)
            {

                float distance = Vector2.Distance(_blackboard.transform.position, _blackboard.path.corners[0]);
                for (int i = 1; i < _blackboard.path.corners.Length; i++)
                {
                    distance += Vector2.Distance(_blackboard.path.corners[i - 1], _blackboard.path.corners[i]);
                }

                Debug.Log("Distance in Enemy Out Of Chase Event: " + distance);

                Debug.Log("Distance to disengage in Enemy Out Of Chase Event: " + _blackboard.DistanceFromPlayerToDisengage);

                if (distance > _blackboard.DistanceFromPlayerToDisengage)
                {
                    return true;
                }
            }

            return false;
        }

        public bool PlayerInDetectionRange()
        {
            // Calculate path from enemy to player
            NavMesh.CalculatePath(new Vector2(_blackboard.transform.position.x, _blackboard.transform.position.y), _blackboard._playerTransform.position,
                NavMesh.AllAreas, _blackboard.path);

            // Do this is the path exists
            if (_blackboard.path.status == NavMeshPathStatus.PathComplete)
            {

                float distance = Vector2.Distance(_blackboard.transform.position, _blackboard.path.corners[0]);
                for (int i = 1; i < _blackboard.path.corners.Length; i++)
                {
                    distance += Vector2.Distance(_blackboard.path.corners[i - 1], _blackboard.path.corners[i]);
                }

                Debug.Log("Distance in player in detection range Event: " + distance);

                if (distance < _blackboard.PlayerDetectionPathLength)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
