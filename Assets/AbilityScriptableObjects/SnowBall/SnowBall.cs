using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SnowBall")]
public class SnowBall : AbilityAbstractClass
{
    private PlayerManager player;
    // [SerializeField] public int buffMult = 3; // this will change to be based on soup value
    // int buffAmount = 10;

    // Create a game object that you can define in the inspector
    [SerializeField] private GameObject snowBallPrefab;
    private GameObject _currentSnowBall;

    // Define Projectile Speed
    [SerializeField] float _projectileSpeed = 100.0f;

    // Define Projectile Lifespan
    [SerializeField] float _lifespan = 3.0f;
       
    // Initialize direction vector
    private Vector2 _projectileDirection;

    public override void Initialize(int soupVal)
    {
        /* Idk what this does*/
        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        _maxUsage = usageValue;
        _remainingUsage = usageValue;
    }
    public override void Active()
    {
        if (_remainingUsage <= 0)
        {
            End();
        }
        else
        {
            // Instantiate snowball at player's position facing the same direction as the player
            _currentSnowBall = Instantiate(snowBallPrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
            _currentSnowBall.transform.up = PlayerManager.instance.player.transform.up;
            _projectileDirection = PlayerManager.instance.player.transform.up;
            _remainingUsage--;
            Debug.Log("Making Snow Ball...");

            // Get its rigidbody component, and set its velocity to the direction it is facing multiplied by the projectile speed.
            if (_currentSnowBall.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = _currentSnowBall.transform.up * _projectileSpeed;
            }
            else
            {
                Debug.LogWarning("Projectile prefab needs a Rigidbody component for movement!");
            }
        }

        Destroy(_currentSnowBall, _lifespan);
    }

    private void Destroy(GameObject objectToDestroy, float delay)
    {
        // TODO: placeholder. Real destruction will be goverened by ability manager and update.
        Object.Destroy(objectToDestroy, delay);
    }

    public override void End()
    {
        // decrease player damage by buff amount
        if (player != null)
        {
            if (_currentSnowBall)
            {
                Destroy(_currentSnowBall);
            }
            PlayerManager.instance.RemoveAbility(this);
        }
        else
        {

        }
    }
}
