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
    [SerializeField] private int numberOfShots = 18;
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
        int rotationDirection = 1;
        public override void Enter(RingerAI ringer)
        {
            rotationDirection = 0;
            ringer.agent.isStopped = false;
        }

        public override void Exit(RingerAI ringer)
        {
            rotationDirection = 0;
            ringer.agent.isStopped = true;
        }

        public override void Update(RingerAI ringer, float deltaT)
        {
            float distance = Vector2.Distance(ringer.transform.position, PlayerEntityManager.Singleton.GetPlayerPosition());
            // If the player is out of range, go back to idle
            if(distance > ringer.followingRadius)
            {
                rotationDirection = 0;
                ringer.currentState.Exit(ringer);
                ringer.currentState = ringer.idle;
                ringer.currentState.Enter(ringer);
            }
            // If the player is too close, flee
            else if (distance < ringer.fleeRadius)
            {
                rotationDirection = 0;
                ringer.agent.speed = ringer.GetMoveSpeed();
                Vector2 holder = ringer.transform.position;
                Vector2 awayFromPlayer = holder - PlayerEntityManager.Singleton.GetPlayerPosition();
                awayFromPlayer.Normalize();
                ringer.agent.SetDestination(holder + (awayFromPlayer * 3));
            }
            // If the player is in range, enter the shooting state
            else if (distance < ringer.shootingRadius)
            {
                if(rotationDirection == 0)
                {
                    if(Random.Range(0, 2) == 0)
                    {
                        rotationDirection = 1;
                    }
                    else
                    {
                        rotationDirection = -1;
                    }
                }
                Vector2 holder = ringer.transform.position;
                Vector2 awayFromPlayer = holder - PlayerEntityManager.Singleton.GetPlayerPosition();
                awayFromPlayer.Normalize();
                Vector2 perpendicular = new Vector2(awayFromPlayer.y, -awayFromPlayer.x) * rotationDirection;
                ringer.agent.speed = ringer.GetMoveSpeed();
                ringer.agent.SetDestination(holder + (perpendicular * 1));
                if(ringer.canShoot())
                {
                    ringer.currentState.Exit(ringer);
                    ringer.currentState = ringer.shooting;
                    ringer.currentState.Enter(ringer);
                }
            }
            // Otherwise, chase the player
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
            ringer.StartCoroutine(ringer.ShootingCoroutine());
            ringer.agent.speed = ringer.GetMoveSpeed()/4;
            ringer.agent.isStopped = true;
        }

        public override void Exit(RingerAI ringer)
        {
            ringer.agent.speed = ringer.GetMoveSpeed();
            ringer.agent.isStopped = false;
            ringer._sprite.color = Color.white;
        }

        public override void Update(RingerAI ringer, float deltaT)
        {
            waitTillShoot -= deltaT;
            if (waitTillShoot <= 0)
            {
                ringer.Shoot();
                float angle = 360 / ringer.numberOfShots;
                for(float i = 0; i <= 360; i += angle)
                {
                    HopShroomSpore bullet = Instantiate(ringer.bullet, ringer.firingPoint.position, Quaternion.Euler(0, 0, i));
                    bullet.SetDirection(new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad)));
                }
                ringer.currentState.Exit(ringer);
                ringer.currentState = ringer.chasing;
                ringer.currentState.Enter(ringer);
            }
        }
    }

    float shootTimer;
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
        shootTimer = timeBetweenShots;

        StartCoroutine(DetectionCoroutine());
    }

    protected override void UpdateAI()
    {
        shootTimer -= Time.deltaTime;
        print(currentState.GetType());
        currentState.Update(this, Time.deltaTime);
    }

    IEnumerator DetectionCoroutine()
    {
        yield return new WaitForSeconds(detectionDelay);
        CheckDetection();
        StartCoroutine(DetectionCoroutine());
    }

    IEnumerator ShootingCoroutine()
    {
        int blinks = (int)(timeToShoot / 0.1f);
        for (int i = 0; i < blinks; i++)
        {
            yield return new WaitForSeconds(0.1f);
            _sprite.color = _sprite.color == Color.white ? Color.green : Color.white;
        }
        _sprite.color = Color.white;
    }

    bool canShoot()
    {
        return shootTimer <= 0;
    }

    void Shoot()
    {
        shootTimer = timeBetweenShots;
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
