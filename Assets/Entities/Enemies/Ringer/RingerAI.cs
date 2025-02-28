using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static Entity;

//The ranged enemy follows player constantly until they are in range, at which point they freeze and fire bullets
//Code tutorial for ranged enemy found at: https://www.youtube.com/watch?v=bwi4lteomic

public class RingerAI : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float detectionRadius = 9f;
    [SerializeField] private float followingRadius = 12f;
    [SerializeField] private float shootingRadius = 6f;
    [SerializeField] private float fleeRadius = 3f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    //[SerializeField] private float distanceToStop = 5f;
    [SerializeField] private float timeToShoot = 0.5f;
    [SerializeField] private float timeAfterShoot = 0.5f;
    [SerializeField] private float timeBetweenShots = 2.0f;
    public Transform firingPoint;
    public HopShroomSpore bullet;
    private Animator animator;

    private RingerState currentState;
    RingerState idle;
    RingerState chasing;
    RingerState shooting;

    public abstract class RingerState
    {
        abstract public void Update(RingerAI ringer, float deltaT);
        abstract public void Enter(RingerAI ringer);
        abstract public void Exit(RingerAI ringer);
    }

    public class IdleState : RingerState
    {
        public override void Enter(RingerAI ringer)
        {
            ringer.agent.isStopped = true;
        }

        public override void Exit(RingerAI ringer)
        {
            ringer.agent.isStopped = false;
        }

        public override void Update(RingerAI ringer, float deltaT)
        {
            if (ringer.playerDetected)
            {
                ringer.currentState.Exit(ringer);
                ringer.currentState = ringer.chasing;
                ringer.currentState.Enter(ringer);
            }
            else
            {
                ringer.agent.SetDestination(ringer.transform.position);
            }
        }
    }

    public class ChasingState : RingerState
    {
        float waitTillShoot = 0.0f;
        public override void Enter(RingerAI ringer)
        {
            waitTillShoot = ringer.timeBetweenShots;
            ringer.agent.isStopped = false;
        }

        public override void Exit(RingerAI ringer)
        {
            ringer.agent.isStopped = true;
        }

        public override void Update(RingerAI ringer, float deltaT)
        {
            waitTillShoot -= deltaT;
            float distance = Vector2.Distance(ringer.transform.position, PlayerEntityManager.Singleton.GetPlayerPosition());
            if(distance < ringer.shootingRadius && waitTillShoot <= 0)
            {
                waitTillShoot = ringer.timeBetweenShots;
                //ringer.currentState.Exit(ringer);
                //ringer.currentState = ringer.shooting;
                //ringer.currentState.Enter(ringer);
            }
            else if(distance > ringer.followingRadius)
            {
                ringer.currentState.Exit(ringer);
                ringer.currentState = ringer.idle;
                ringer.currentState.Enter(ringer);
            }
            else if (distance < ringer.fleeRadius)
            {
                ringer.agent.speed = ringer.GetMoveSpeed() * 2;
                Vector2 holder = ringer.transform.position;
                Vector2 awayFromPlayer = holder - PlayerEntityManager.Singleton.GetPlayerPosition();
                awayFromPlayer.Normalize();
                ringer.agent.SetDestination(holder + (awayFromPlayer * 3));
            }
            else
            {
                ringer.agent.speed = ringer.GetMoveSpeed();
                ringer.agent.SetDestination(PlayerEntityManager.Singleton.GetPlayerPosition());
            }
        }
    }

    public class ShootingState : RingerState
    {
        float waitTillShoot = 0.0f;
        public override void Enter(RingerAI ringer)
        {
            waitTillShoot = ringer.timeToShoot;
            ringer.agent.isStopped = true;
        }

        public override void Exit(RingerAI ringer)
        {
            ringer.agent.isStopped = false;
        }

        public override void Update(RingerAI ringer, float deltaT)
        {
            print("IMMA FIRING MY LAZERS");
            return;
            waitTillShoot -= deltaT;
            if(waitTillShoot <= 0)
            {
                // ringer.Shoot();
                waitTillShoot = ringer.timeBetweenShots;
            }
            float distance = Vector2.Distance(ringer.transform.position, PlayerEntityManager.Singleton.GetPlayerPosition());
            if(distance > ringer.shootingRadius)
            {
                ringer.currentState.Exit(ringer);
                ringer.currentState = ringer.chasing;
                ringer.currentState.Enter(ringer);
            }
        }
    }
    

    protected void Start()
    {
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();

        idle = new IdleState();
        chasing = new ChasingState();
        shooting = new ShootingState();

        currentState = idle;

        StartCoroutine(DetectionCoroutine());
    }

    protected override void UpdateAI()
    {
        print(currentState.GetType());
        currentState.Update(this, Time.deltaTime);
    }

    IEnumerator DetectionCoroutine()
    {
        yield return new WaitForSeconds(detectionDelay);
        CheckDetection();
        StartCoroutine(DetectionCoroutine());
    }

    protected void CheckDetection()
    {
        Collider2D collider;
        if (playerDetected)
        {
            collider = Physics2D.OverlapCircle((Vector2)transform.position, followingRadius, playerLayer);
        }
        else
        {
            collider = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
        }
        if (collider != null)
        {
            playerDetected = true;
        }
        else
        {
            playerDetected = false;
        }
    }
}
