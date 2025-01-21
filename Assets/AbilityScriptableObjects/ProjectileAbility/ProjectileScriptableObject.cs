using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To create a projectile scriptable object, just go to the project overview, right click, click "create", and
// at the top should be a menu titled "abilities". Should be under that menu.
[CreateAssetMenu(menuName = "Abilities/Projectile")]
public class ProjectileScriptableObject : AbilityAbstractClass
{
    [SerializeField]
    private List<GameObject> _projectilePool;

    public GameObject projectilePrefab;
    private Vector2 _projectileDirection;

    [SerializeField] float _projectileSpeed = 100.0f;

    // TODO: remove once ability manager is implemented
    [SerializeField] float _lifespan = 3.0f;

    public override void Initialize(int soupVal)
    {
        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;

        Debug.Log("Projectiles Initialized!!");

        for(int i = 0; i < _maxUsage; i++)
        {
            // create all objects in the pool, and deactivate all
            _projectilePool.Add(Instantiate(projectilePrefab));
            _projectilePool[i].SetActive(false);
        }
    }

    private void startProjectile(GameObject go, GameObject player) {
        // Spawn projectile at player's position, and then set its rotation to be facing the same direction as the player.
        go.SetActive(true);
        go.transform.position = player.transform.position;
        go.transform.up = player.transform.up;
        _projectileDirection = player.transform.up;

        // Get its rigidbody component, and set its velocity to the direction it is facing multiplied by the projectile speed.
        if (go.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = go.transform.up * _projectileSpeed;
        }
        else
        {
            Debug.LogWarning("Projectile prefab needs a Rigidbody component for movement!");
        }
    }

    public override void Active()
    {   
        // Check if ability has time, otherwise call End()
        if(_remainingUsage <= 0)
        {
            End();
            return;
        } else 
        {
            // Get an inactive object from the pool
            GameObject go = GetPooledObject();
            if (go != null)
            {
                // make the projectile active in heirarchy and start its journey

                // Comment: odd business, mostly because why pool? feels useless lowkey other than the loading all objects at once.... have to bring this up later
                startProjectile(go, PlayerManager.instance.player);
                // Timer helper object HUNTS DOWN the object and MURDERS it after the delay.
                TimerHelper.instance.DeactivateAfterDelay(go, _lifespan);
                _remainingUsage--;
            }
        }
    }

    public override void End()
    {
        for (int i = 0; i < _projectilePool.Count; i++)
        {
            if( _projectilePool[i] != null )
            {
                Destroy(_projectilePool[i]);
            }
        }
        PlayerManager.instance.RemoveAbility(this);
    }

    private GameObject GetPooledObject()
    {
        // loop over the object pool
        for (int i = 0; i < _projectilePool.Count; i++)
        {
            // check if there are any objects that are not currently active
            if (!_projectilePool[i].activeInHierarchy)
            {
                // if object is inactive, return it for usage :P
                return _projectilePool[i];
            }
        }
        return null;
    }

}
