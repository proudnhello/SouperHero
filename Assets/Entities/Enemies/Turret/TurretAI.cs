// portions of this file were generated using GitHub Copilot
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

public class TurretAI : EnemyBaseClass
{
    [Header("Player Detection")]
    private bool playerDetected = false;
    [SerializeField] private float shootingRadius = 9f;
    [SerializeField] protected LayerMask playerLayer;
    //[SerializeField] private float distanceToStop = 5f;
    [SerializeField] private float emergeTime = 1;
    [SerializeField] private float hideTime = 2;
    [SerializeField] private float timeBetweenShots = 0.3f;
    [SerializeField] private int numberOfShots = 3;
    [SerializeField] private float hideAlphaMultiplier = 0.1f;
    private float detectionDelay = 0.3f;
    public Transform firingPoint;
    public HopShroomSpore bullet;
    private Animator animator;

    private TurretState currentState;
    private HidingState hiding;
    private EmergeState emerging;
    private ShootState shooting;

    // The collider should be off when the turret is hiding, and on when it is emerging or shooting. Entering state will handle this, exits will not
    Collider2D c;
    SpriteRenderer sprite;
    Color initialColor;

    private bool emerged = false;

    public abstract class TurretState
    {
        abstract public void Update(TurretAI turret, float deltaT);
        abstract public void Enter(TurretAI turret, TurretState previousState);
        abstract public void Exit(TurretAI turret);
    }

    // This is the state where the turret is hidden and not shooting, and as such, cannot take damage
    public class HidingState : TurretState
    {
        float hideTimer = 0;

        public override void Enter(TurretAI turret, TurretState _previousState)
        {
            turret.animator.Play("Retreating");
            turret.c.enabled = false;
            print(turret.c.enabled);
            hideTimer = turret.hideTime;
        }

        public override void Update(TurretAI turret, float deltaT)
        {
            hideTimer -= deltaT;
            if((turret.playerDetected || turret.alwaysAggro) && hideTimer <= 0)
            {
                turret.animator.Play("Emerge");
                turret.SwapState(turret.emerging);
            }
        }

        public override void Exit(TurretAI turret)
        {
            return;
        }
    }

    // This is the state where the turret is emerging from the ground/retreating back into the ground, not shooting, and can take damage
    public class EmergeState : TurretState
    {
        float emergeTimer = 0;
        TurretState initialState;
        public override void Enter(TurretAI turret, TurretState previousState)
        {
            turret.c.enabled = true;
            initialState = previousState;
            emergeTimer = turret.emergeTime;
            turret.sprite.color = turret.initialColor;
            return;
        }

        public override void Update(TurretAI turret, float deltaT)
        {
            emergeTimer -= deltaT;

            if (emergeTimer <= 0)
            {
                if(initialState == turret.shooting)
                {
                    turret.SwapState(turret.hiding);
                }
                else
                {
                    turret.SwapState(turret.shooting);
                }
            }
            return;
        }

        public override void Exit(TurretAI turret)
        {
            return;
        }
    }

    // This is the state where the turret is shooting at the player and can take damage
    public class ShootState : TurretState
    {
        int remainingShots;
        float shotTimer;
        public override void Enter(TurretAI turret, TurretState _previousState)
        {
            turret.c.enabled = true;
            remainingShots = turret.numberOfShots;
            shotTimer = turret.timeBetweenShots;
            turret.sprite.color = turret.initialColor;
            return;
        }

        public override void Update(TurretAI turret, float deltaT)
        {
            shotTimer -= deltaT;
            if(shotTimer <= 0)
            {
                turret.Shoot();
                turret.animator.Play("Attack");
                remainingShots--;
                shotTimer = turret.timeBetweenShots;
            }
            if(remainingShots <= 0)
            {
                turret.SwapState(turret.emerging);
            }
            return;
        }

        public override void Exit(TurretAI turret)
        {
            return;
        }
    }

    protected override void UpdateAI()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        agent.updatePosition = false;
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        currentState.Update(this, Time.deltaTime);
    }

    private void SwapState(TurretState newState)
    {
        TurretState previousState = currentState;
        currentState.Exit(this);
        currentState = newState;
        currentState.Enter(this, previousState);
    }

    IEnumerator DetectionCoroutine()
    {
        yield return new WaitForSeconds(detectionDelay);
        CheckDetection();
        StartCoroutine(DetectionCoroutine());
    }

    protected void CheckDetection()
    {
        Collider2D collider = Physics2D.OverlapCircle((Vector2)transform.position, shootingRadius, playerLayer);
        
        if (collider != null)
        {
            playerDetected = true;
        }
        else
        {
            playerDetected = false;
        }
    }

    public void Shoot()
    {
        animator.Play("Attack");
        HopShroomSpore bulletInstance = Instantiate(bullet, firingPoint.position, firingPoint.rotation);
        Vector2 playerDirection = PlayerEntityManager.Singleton.transform.position - transform.position;
        bulletInstance.SetDirection(playerDirection);
    }

    public void Start()
    {
        initEnemy();
        hiding = new HidingState();
        emerging = new EmergeState();
        shooting = new ShootState();

        StartCoroutine(DetectionCoroutine());

        currentState = hiding;
        c = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        initialColor = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a);

        animator = GetComponent<Animator>();

        hiding.Enter(this, null);
    }

}
