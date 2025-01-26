using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Code tutorial for ranged enemy found at: https://www.youtube.com/watch?v=bwi4lteomic

public class EnemyRanged : EnemyBaseClass
{
    [SerializeField]
    private float newMoveSpeed = 1f;

    //[SerializeField] private float distanceToStop = 5f;
    [SerializeField] private float distanceToShoot;
    private float fireRate = 1f;
    private float rotateSpeed = 0.01f;
    private float timeToFire = 0f;

    public Transform firingPoint;
    public GameObject EnemyBullet;


    protected new void Start()
    {
        base.Start();
        moveSpeed = newMoveSpeed;
        currentHealth = maxHealth;
    }
    protected override void UpdateAI()
    {
        RotateTowardsPlayer();
        //Check if player is alive and within the attack distance
        if (Vector2.Distance(playerTransform.position, transform.position) >= distanceToShoot)
        {
            Follow();
        } else
        {
            _rigidbody.velocity = Vector2.zero; //Freeze enemy
            Shoot();
        }
    }

    protected new void Update()
    {
        base.Update();
    }

    private void Follow() //Code re-used from EnemyCharger script
    {
        Vector2 targetDirection = playerTransform.position - transform.position;
        targetDirection = targetDirection.normalized;
        targetDirection *= moveSpeed;
        _rigidbody.velocity = targetDirection;
    }

    private void Shoot()
    {
        if(timeToFire <= 0f)
        {
            Instantiate(EnemyBullet, firingPoint.position, firingPoint.rotation);
            timeToFire = fireRate;
        } else
        {
            timeToFire -= Time.deltaTime;
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector2 targetDirection = playerTransform.position - transform.position;
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion q = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, q, rotateSpeed);
    }
}
