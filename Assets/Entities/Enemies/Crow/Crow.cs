using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RingerAI;
using UnityEngine.UIElements.Experimental;
using Unity.VisualScripting;
using static UnityEngine.GridBrushBase;

// This file was edited with help from GitHub Copilot

public class Crow : EnemyBaseClass
{
    protected CrowState currentState;
    Animator animator;
    [Header("Detection Settings")]
    [SerializeField] protected float retreatRadius = 5f; // Enemy runs away from the player if they are within this radius
    [SerializeField] protected float bombRadius = 9f; // If enemy lands within this radius of the player, it will drop a bomb
    [SerializeField] protected float detectionRadius = 12f; // Enemy will detect the player within this radius
    [SerializeField] protected float followingRadius = 15f; // Enemy will follow the player if they are within this radius
    [SerializeField] protected float detectionDelay = 0.2f; // Delay between detection checks
    [SerializeField] protected LayerMask playerLayer;

    [Header("Movement Settings")]
    [SerializeField] protected float hopDelay = 0.2f; // Delay between hops
    [SerializeField] protected float hopDistance = 4f; // Distance the crow hops at a time

    [Header("Bomb Settings")]
    [SerializeField] protected GameObject bombPrefab; // Prefab of the bomb to drop
    GameObject bomb;
    [SerializeField] protected float bombTimer = 1.5f; // Time before the bomb explodes after being dropped
    [SerializeField] protected float bombDamage = 20f; // Damage dealt by the bomb
    [SerializeField] protected float bombSize = 6f; // Size of the bomb explosion

    bool playerDetected = false;
    public abstract class CrowState
    {
        abstract public void Update(Crow crow, float deltaT);
        abstract public void Enter(Crow crow);
        abstract public void Exit(Crow crow);
    }

    public class IdleState : CrowState
    {
        public override void Update(Crow crow, float deltaT)
        {
            // If the crow is not hopping, find a new location within a certain radius to hop to
            if (!crow.hopping)
            {
                crow.CurrentTarget = new Vector2(crow.transform.position.x, crow.transform.position.y) + (Random.insideUnitCircle.normalized * crow.hopDistance/3);
            }
            if (crow.playerDetected)
            {
                // If the player is detected, switch to follow state
                crow.currentState.Exit(crow);
                crow.currentState = crow.followState;
                crow.currentState.Enter(crow);
            }
        }

        public override void Enter(Crow crow)
        {
            crow.CurrentTarget = crow.transform.position; // Set the target to the current position when entering idle state
        }

        public override void Exit(Crow crow)
        {
            return;
        }
    }


    public class FollowState : CrowState
    {
        public override void Update(Crow crow, float deltaT)
        {
            if (!crow.hopping)
            {
                crow.CurrentTarget = crow._playerTransform.position;
            }
            float distance = Vector2.Distance(crow.transform.position, crow._playerTransform.position);
            if (distance < crow.retreatRadius)
            {
                // If the player is too close, switch to retreat state
                crow.currentState.Exit(crow);
                crow.currentState = crow.retreatState;
                crow.currentState.Enter(crow);
            }
            else if (distance > crow.followingRadius)
            {
                // If the player is too far, switch to idle state
                crow.currentState.Exit(crow);
                crow.currentState = crow.idleState;
                crow.currentState.Enter(crow);
            }
        }

        public override void Enter(Crow crow)
        {
            return; 
        }

        public override void Exit(Crow crow)
        {
            return;
        }
    }

    public class RetreatState : CrowState
    {
        public override void Update(Crow crow, float deltaT)
        {
            if (!crow.hopping)
            {
                Vector2 holder = (Vector2)crow.transform.position;
                Vector2 awayFromPlayer = holder - (Vector2)crow._playerTransform.position;
                awayFromPlayer.Normalize();
                crow.CurrentTarget = holder + awayFromPlayer * crow.hopDistance; // Set the target to hop away from the player
            }
            float distance = Vector2.Distance(crow.transform.position, crow._playerTransform.position);
            // Drop a bomb if
            if (
                crow.playerDetected && // We can see the player
                Vector2.Distance(crow.transform.position, PlayerEntityManager.Singleton.GetPlayerPosition()) < crow.bombRadius && // The player is within the bomb radius
                (crow.bomb == null || !crow.bomb.activeInHierarchy)) // The current bomb has blown up
            {
                // Drop a bomb
                crow.animator.Play("Caw");
                crow.bomb = Instantiate(crow.bombPrefab, crow.transform.position, Quaternion.identity);
                LandmineObject landmine = crow.bomb.GetComponent<LandmineObject>();
                if (landmine != null)
                {
                    landmine.init(crow.bombSize);
                    landmine.StartCoroutine(landmine.Detonate(crow.bombTimer, crow.bombSize, crow.bombDamage));
                }
            }
            if (distance > crow.retreatRadius && (crow.bomb == null || !crow.bomb.activeInHierarchy))
            {
                // If they're a bit far and the bomb has blown up, switch to follow state
                crow.currentState.Exit(crow);
                crow.currentState = crow.followState; 
                crow.currentState.Enter(crow);
            }
            else if (distance > crow.followingRadius)
            {
                // If the player is too far, switch to idle state
                crow.currentState.Exit(crow);
                crow.currentState = crow.idleState;
                crow.currentState.Enter(crow);
            }
        }

        public override void Enter(Crow crow)
        {
            return;
        }

        public override void Exit(Crow crow)
        {
            return;
        }
    }

    /* Doesn't reallt work tbh, so it's not used right now
    public class CircleState : CrowState {    
        int circleDirection = 1; // 1 for clockwise, -1 for counter-clockwise
        public override void Update(Crow crow, float deltaT)
        {
            // Hop in a circular pattern around the player, using the circleDirection to determine the direction of the hop
            if (!crow.hopping)
            {
                Vector2 playerPosition = (Vector2)crow._playerTransform.position;
                Vector2 direction = (Vector2)crow.transform.position - playerPosition;
                direction.Normalize();
                float angle = Mathf.Atan2(direction.y, direction.x) + (Mathf.PI / 4 * circleDirection); // Adjust the angle based on the circle direction
                crow.CurrentTarget = playerPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * crow.hopDistance; // Set the target to hop in a circular pattern
            }
        }

        public override void Enter(Crow crow)
        {
            if (Random.Range(0, 2) == 0)
            {
                circleDirection = 1;
            }
            else
            {
                circleDirection = -1;
            }
        }

        public override void Exit(Crow crow)
        {
            return;
        }
    }
    */

    Vector2 CurrentTarget;
    bool hopping;
    // Constant coroutine for hopping towards a target defined by the current state
    protected IEnumerator HopCoroutine()
    {
        Vector2 target = Vector2.zero;
        while (!IsDead())
        {
            if(agent == null || !agent.isOnNavMesh)
            {
                yield return new WaitForSeconds(0.1f); // Wait a bit before trying again if the agent is not ready
                continue;
            }
            if (target != CurrentTarget)
            {
                agent.SetDestination(CurrentTarget);
                target = CurrentTarget;
            }

            yield return Hop();

            yield return new WaitForSeconds(hopDelay);
        }
    }

    // Coroutine that does the actual hopping movement
    protected IEnumerator Hop()
    {
        hopping = true;
        float initialDistance = agent.remainingDistance;
        agent.isStopped = false; // Ensure the agent is moving
        float currentDistance = initialDistance;
        while (initialDistance > 0.1f && initialDistance - agent.remainingDistance < hopDistance)
        {
            currentDistance = agent.remainingDistance;
            yield return new WaitForSeconds(0.5f); // Wait for the agent to move
            if (currentDistance - agent.remainingDistance < 0.0001f) // If the agent hasn't moved, break the loop
            {
                hopping = false;
                agent.isStopped = true; // Stop the agent if it hasn't moved
                break;
            }
        }
        agent.isStopped = true; // Stop the agent after reaching the target position
        hopping = false;
    }

    protected IdleState idleState;
    protected FollowState followState;
    protected RetreatState retreatState;
    //protected CircleState circleState;

    protected void Start()
    {
        initEnemy();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = GetMoveSpeed();
        idleState = new IdleState();
        followState = new FollowState();
        retreatState = new RetreatState();
        //circleState = new CircleState();
        currentState = idleState;

        animator = GetComponent<Animator>();
        animator.Play("Hop");

        StartCoroutine(DetectionCoroutine());
        StartCoroutine(HopCoroutine());

        CurrentTarget = transform.position;
    }

    protected override void UpdateAI()
    {
        currentState?.Update(this, Time.deltaTime);
    }

    protected override void Die()
    {
        currentState.Exit(this);
        currentState = null;
        base.Die();
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
            if (playerDetected == false) AudioManager.Singleton._MusicHandler.AddAgro(enemyIndex);
            playerDetected = true;
        }
        else
        {
            if (playerDetected == true) AudioManager.Singleton._MusicHandler.RemoveAgro(enemyIndex);
            playerDetected = false;
        }
    }
}
