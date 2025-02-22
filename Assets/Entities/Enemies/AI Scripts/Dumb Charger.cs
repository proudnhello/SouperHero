using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class DumbCharger : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float followingRadius = 6f;
    private float detectionDelay = 0.3f;
    [SerializeField] protected LayerMask playerLayer;
    void Start(){
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        StartCoroutine(DetectionCoroutine());
    }
    void Update(){
        if (IsDead()) return;
        //print(agent.destination);

        if (playerDetected)
        {
            UpdateAI();
        }
        else Patrol();
    }
    protected override void UpdateAI(){
        if(IsDead()) return;
        agent.SetDestination(_playerTransform.position);

        if(agent.destination.x < transform.position.x){
            _sprite.flipX = true;
        }
        else{
            _sprite.flipX = false;
        }
    }

    // Player Detection
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

    protected void Patrol()
    {
        _rigidbody.velocity = Vector2.zero;
    } 

    private void OnDrawGizmos() //Testing only
    {
        //Lo: Toggle Gizmos on in game view to see radius
        //Display detection radius on enemies
        Gizmos.color = new Color(255, 0, 0, 0.25f);
        if (playerDetected) Gizmos.color = new Color(0, 255, 0, 0.25f);
        Gizmos.DrawSphere((Vector2)transform.position, detectionRadius);
    }
}
