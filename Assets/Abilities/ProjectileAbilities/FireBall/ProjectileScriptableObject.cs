using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

// To create a projectile scriptable object, just go to the project overview, right click, click "create", and
// at the top should be a menu titled "abilities". Should be under that menu.
[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileScriptableObject : AbilityAbstractClass
{

    public GameObject projectilePrefab;
    private GameObject _currentProjectile;

    private Vector2 _projectileDirection;

    [SerializeField] float _baseProjectileSpeed = 10.0f;

    // TODO: remove once ability manager is implemented
    [SerializeField] float _lifespan = 3.0f;
    [SerializeField] float _radiusMultiplier = 0.5f;

    public override void Initialize(int soupVal)
    {
        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
    }
    public override void Active()
    {   
        // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
        _currentProjectile = Instantiate(projectilePrefab, PlayerManager.Singleton.player.transform.position, Quaternion.identity);
        _currentProjectile.transform.up = PlayerManager.Singleton.player.transform.up;
        _projectileDirection = PlayerManager.Singleton.player.transform.up;
        _currentProjectile.GetComponent<FireBallCollision>().stats = stats;
        _currentProjectile.GetComponent<FireBallCollision>().statusEffects = statusEffects;

        float radius = stats.size * _radiusMultiplier;
        _currentProjectile.transform.localScale = new Vector3(radius, radius, radius);

        float speed = stats.speed * _baseProjectileSpeed;

        _currentProjectile.GetComponent<FireBallCollision>().despawnTime = stats.duration;

        // Get its rigidbody component, and set its velocity to the direction it is facing multiplied by the projectile speed.
        if (_currentProjectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = _currentProjectile.transform.up * speed;
        }
        else
        {
            Debug.LogWarning("Projectile prefab needs a Rigidbody component for movement!");
        }

    }


    public override void End()
    {
        //PlayerManager.instance.RemoveAbility(this);
    }


}