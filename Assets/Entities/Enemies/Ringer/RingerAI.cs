using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
    private float windUp = 0.0f;
    private float timeSinceLastShot = 0.0f;
    public Transform firingPoint;
    public HopShroomSpore bullet;
    private Animator animator;

    RingerState state;
    RingerState idle;
    RingerState chasing;
    RingerState shooting;

    private abstract class RingerState
    {
        protected RingerAI ringer;
        abstract public void Update(RingerAI ringer);
        abstract public void Enter(RingerAI ringer);
        abstract public void Exit(RingerAI ringer);
    }


    protected void Start()
    {
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();

    }

    protected override void UpdateAI()
    {
        return;
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
