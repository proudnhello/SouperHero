using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The ranged enemy follows player constantly until they are in range, at which point they freeze and fire bullets
//Code tutorial for ranged enemy found at: https://www.youtube.com/watch?v=bwi4lteomic

public class EnemyRanged : EnemyBaseClass
{
    [SerializeField]
    private float newMoveSpeed = 1f;

    //[SerializeField] private float distanceToStop = 5f;
    [SerializeField] private float distanceToShoot; //Lo: Stop and shoot distance are currently the same
    private float fireRate = 0.5f;
    private float rotateSpeed = 0.1f;
    private float timeToFire = 0f;
    private Vector2 targetDirection;

    public Transform firingPoint;
    public GameObject EnemyBullet;


    protected new void Start()
    {
        base.Start();
    }
    protected override void UpdateAI()
    {
        RotateTowardsPlayer();
        //Check if player is alive and within the attack distance
        if (Vector2.Distance(_playerTransform.position, transform.position) >= distanceToShoot)
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
        targetDirection = targetDirection.normalized;
        targetDirection *= GetMoveSpeed();
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
        targetDirection = _playerTransform.position - transform.position;
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion q = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, q, rotateSpeed);
    }
}
