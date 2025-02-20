using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

//The ranged enemy follows player constantly until they are in range, at which point they freeze and fire bullets
//Code tutorial for ranged enemy found at: https://www.youtube.com/watch?v=bwi4lteomic

public class Ranger : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float detectionRadius = 9f;
    [SerializeField] private float followingRadius = 12f;
    [SerializeField] private float shootingRadius = 6f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    //[SerializeField] private float distanceToStop = 5f;
    [SerializeField] private float timeToShoot = 0.5f;
    [SerializeField] private float timeAfterShoot = 0.3f;
    [SerializeField] private float timeBetweenShots = 2.0f;
    public Transform firingPoint;
    public Bullet bullet;
    private Animator animator;

    private enum RangerState
    {
        IDLING,
        CHASING,
        PREPARING,
        SHOOTING
    }
    private RangerState _state;

    protected void Start()
    {
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        _state = RangerState.IDLING;
        animator = GetComponent<Animator>();
        StartCoroutine(DetectionCoroutine());
    }

    protected void Update()
    {
        if (IsDead()) return;

        if (playerDetected)
        {
            UpdateAI();
        }

        switch(_state){
            case RangerState.IDLING:
                animator.Play("Idle");
                break;
            case RangerState.CHASING:
                animator.Play("Walk");
                break;
            case RangerState.PREPARING:
                animator.Play("Walk");
                break;
            case RangerState.SHOOTING:
                animator.Play("Attack");
                break;
        }
    }
    protected override void UpdateAI()
    {
        float distance = Vector2.Distance(_playerTransform.position, transform.position);
        if(distance < followingRadius){
            agent.SetDestination(_playerTransform.position);
            if(_state != RangerState.SHOOTING && _state != RangerState.PREPARING){
                StartCoroutine(ChasingCoroutine());
            }
        }
        else{
            _state = RangerState.IDLING;
        }

        if(_state != RangerState.SHOOTING){
            if(agent.destination.x < transform.position.x){
                _sprite.flipX = true;
            }
            else{
                _sprite.flipX = false;
            }   
        }
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
        if(playerDetected){
            collider = Physics2D.OverlapCircle((Vector2)transform.position, followingRadius, playerLayer);
        }
        else{
            collider = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
        }
        if (collider != null)
        {
            playerDetected = true;
        } else
        {
            playerDetected = false;
        }
    }

    IEnumerator ChasingCoroutine(){
        // Wait a few seconds before shooting
        _state = RangerState.PREPARING;
        yield return new WaitForSeconds(timeBetweenShots);

        // Play shooting animation and set state
        _state = RangerState.SHOOTING;
        agent.isStopped = true;
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        // wait til animation is finshed then make bullet       
        yield return new WaitForSecondsRealtime(animationLength);
        Bullet temp = Instantiate(bullet, firingPoint.position, firingPoint.rotation);
        temp.direction = (_playerTransform.position - transform.position).normalized;

        // clean up
        agent.isStopped = false;
        _state = RangerState.CHASING;
    }
}
