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
    private float _freezeEnemyWhenThisFar = 38f;
    private float _playerDetectionIntervalWhenFrozen = 1.5f;

    // Idle State
    private float _playerDetectionPathLength = 6.5f;
    private float _playerDetectionInterval = .5f;
    private Vector2 _patrolDistance = new Vector2(3f, 10f);
    private Vector2 _patrolWaitTime = new Vector2(1f, 3f);
    private float _whilePatrolCheckIfStoppedInterval = .15f;
    private float _whilePatrolCheckIfStoppedDistance = .07f;
    private float _idleSpeedMultiplier = 1f;

    // Attack State
    private float _attackSpeedMultiplier = 1.5f;
    private float _distanceToPlayerForCharge = 5.0f;
    private float _attackDistanceCheckInterval = 0.3f;
    private float _chargeSpeed = 6.88f;
    public float _chargeForce = 106080f;
    private float _chargeTime = 0.48f;
    private int _numConsecutiveCharges = 3;
    private Vector2 _chargeCooldownTime = new Vector2(.3f, .4f);
    private float _finalChargeCooldownTime = 1.62f;
    // NOTE: This should always be higher than PlayerDetectionPathLength
    private float _distanceFromPlayerToDisengage = 7f;

    // Consecutive Charge State
    int _chargesCounter;
    public int ChargesCounter { get { return _chargesCounter; } set { _chargesCounter = value; } }

    bool _consecutiveChargeIsActive;
    public bool ConsecutiveChargeIsActive { get { return _consecutiveChargeIsActive; } set { _consecutiveChargeIsActive = value; } }


    // Getters and Setters
    public float FreezeEnemyWhenThisFar { get { return _freezeEnemyWhenThisFar; } }
    public float PlayerDetectionIntervalWhenFrozen { get { return _playerDetectionIntervalWhenFrozen; } }
    public float PlayerDetectionPathLength { get { return _playerDetectionPathLength; } }
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
    public int NumConsecutiveCharges {  get { return _numConsecutiveCharges; } }
    public Vector2 ChargeCooldownTime {  get { return _chargeCooldownTime; } }
    public float FinalChargeCooldownTime { get { return _finalChargeCooldownTime; } }
    public float DistanceFromPlayerToDisengage {  get { return _distanceFromPlayerToDisengage;  } }



    public Animator animator;
    internal List<IState> states;
    internal IState currentState;

    public NavMeshAgent Agent { get { return agent; } }
    public Transform PlayerTransform { get { return _playerTransform; } }
    public bool AlwaysAggro { get { return alwaysAggro; } }

    public StateMachine stateMachine;

    public StateFactory stateFactory;

    public StateMachineEvents Events;

    NavMeshPath path;

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

        _sprite.flipX = agent.destination.x > transform.position.x || _rigidbody.velocity.x > 0;
    }

    protected override void Die()
    {
        stateMachine.CurrentState.ExitState();
        base.Die();
    }

    float _startTime = -1;
    float _waitSeconds;
    // Returns True if Timer hasn't started
    // Returns False if Timer has started
    // Starts Timer if timer hasn't started
    public bool StartWaitTimer(float waitSeconds)
    {
        if (!HasTimerStarted())
        {
            _waitSeconds = waitSeconds;
            _startTime = Time.time;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasTimerStarted()
    {
        if (_startTime < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // returns false if time isn't up or time hasn't started
    public bool CheckWaitTimer()
    {
        // check if timer hasn't started
        if (_startTime == -1f)
        {
            return false;
        }

        /// check if wait time has elapsed
        if (_waitSeconds < Math.Abs(_startTime - Time.time))
        {
            _startTime = -1;
            return true;
        }
        else
        {
            return false;
        }
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
            float distance = NavMeshDist();

            if (distance < 0)
            {
                return false;
            }
            else if (distance > _blackboard.DistanceFromPlayerToDisengage)
            {
                return true;
            }

            return false;
        }

        public bool PlayerInDetectionRangeEvent()
        {
            float distance = NavMeshDist();

            if (distance < 0)
            {
                return false;
            }
            else if (distance < _blackboard.PlayerDetectionPathLength)
            {
                return true;
            }

            return false;
        }

        public bool PlayerInChargeRangeEvent()
        {
            float distance = NavMeshDist();

            if (distance < 0)
            {
                return false;
            }
            else if (distance < _blackboard.DistanceToPlayerForCharge)
            {
                return true;
            }

            return false;
        }

        public bool PlayerOutOfFrozenRangeEvent()
        {
            return EuclideanDist() > _blackboard.FreezeEnemyWhenThisFar;
        }


        // returns -1 if navmesh not complete
        private float NavMeshDist()
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

                return distance;
            }

            return -1;
        }

        private float EuclideanDist()
        {
            return Vector2.Distance(_blackboard.transform.position, PlayerEntityManager.Singleton.transform.position);
        }

    }
}
