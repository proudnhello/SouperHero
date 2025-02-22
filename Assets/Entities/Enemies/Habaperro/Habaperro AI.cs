using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HabaperroAI : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float detectionRadius = 9f;
    [SerializeField] private float followingRadius = 12f;
    [SerializeField] private float attackingRadius = 3f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    private Animator animator;

    private enum HabaperroState
    {
        IDLING,
        CHASING,
        PREPARING,
        ATTACKING,
        BOOM
    }
    private HabaperroState _state;

    void Start()
    {
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        _state = HabaperroState.IDLING;
        animator = GetComponent<Animator>();
        StartCoroutine(DetectionCoroutine());
    }

    void Update()
    {
        if (IsDead()) return;

        if (playerDetected)
        {
            UpdateAI();
        }

        switch(_state){
            case HabaperroState.IDLING:
                animator.Play("Idle");
                break;
            case HabaperroState.CHASING:
                animator.Play("Walk");
                break;
        }
    }

    protected override void UpdateAI()
    {
        if (!agent.enabled)
        {
            return;
        }
        if(_state == HabaperroState.PREPARING || _state == HabaperroState.ATTACKING || _state == HabaperroState.BOOM){
            return;
        }
        float distance = Vector2.Distance(_playerTransform.position, transform.position);
        if(distance < followingRadius){
            if(_state != HabaperroState.ATTACKING && _state != HabaperroState.PREPARING){
                agent.SetDestination(_playerTransform.position);
                if(distance < attackingRadius){
                    StartCoroutine(ExplodingCoroutine());
                }
            }
        }
        else{
            _state = HabaperroState.IDLING;
        }

        if(_state != HabaperroState.ATTACKING){
            if(agent.destination.x > transform.position.x){
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

    IEnumerator ExplodingCoroutine(){
        agent.isStopped = true;
        _state = HabaperroState.PREPARING;
        animator.Play("Sit");
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);

        _state = HabaperroState.ATTACKING;  
        animator.Play("Ignite");
        animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);

        _state = HabaperroState.BOOM;  
        animator.Play("Boom");
        animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);

        Destroy(gameObject);
    }

}
