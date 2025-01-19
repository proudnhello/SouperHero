using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To create a projectile scriptable object, just go to the project overview, right click, click "create", and
// at the top should be a menu titled "abilities". Should be under that menu.
[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileScriptableObject : AbilityAbstractClass
{

    public GameObject projectilePrefab;
    private GameObject _currentProjectile;

    private Vector2 _projectileDirection;

    [SerializeField] float _projectileSpeed = 100.0f;

    // TODO: remove once ability manager is implemented
    [SerializeField] float _lifespan = 3.0f;
    bool isActive = false;  

    public override void Initialize(int soupVal)
    {
        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;
        isActive = true;
    }
    public override void Active()
    {   
        // Check if ability has time, otherwise call End()
        if(_remainingUsage <= 0)
        {
            End();
            return;
        }
        // If its the first time calling Active(), then spawn the projectile
        if (_remainingUsage == _maxUsage)
        {
            // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
            _currentProjectile = Instantiate(projectilePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
            _currentProjectile.transform.up = PlayerManager.instance.player.transform.up;
            _projectileDirection = PlayerManager.instance.player.transform.up;

            // Get its rigidbody component, and set its velocity to the direction it is facing multiplied by the projectile speed.
            if (_currentProjectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = _currentProjectile.transform.up * _projectileSpeed;
            }
            else
            {
                Debug.LogWarning("Projectile prefab needs a Rigidbody component for movement!");
            }
        }
        //TODO: _remainingUsage--;
        Destroy(_currentProjectile, _lifespan);
    }

    private void Destroy(GameObject objectToDestroy, float delay)
    {
        // TODO: placeholder. Real destruction will be goverened by ability manager and update.
        Object.Destroy(objectToDestroy, delay);
    }

    public override void End()
    {
        if(!isActive)
        {
            return;
        }
        if(_currentProjectile)
        {
            Destroy(_currentProjectile);
        }
        isActive = false;
    }


}
