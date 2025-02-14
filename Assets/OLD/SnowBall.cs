using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Abilities/SnowBall")]
//public class SnowBall : AbilityAbstractClass
//{
//    private PlayerManager player;

//    // Create a game object that you can define in the inspector
//    [SerializeField] private GameObject snowBallPrefab;
//    private GameObject _currentSnowBall;

//    // Define Projectile Speed
//    [SerializeField] float _projectileSpeed = 100.0f;

       
//    // Initialize direction vector
//    private Vector2 _projectileDirection;

//    public override void Initialize(int soupVal)
//    {
//        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
//    }
//    public override void UseAbility()
//    {
        
//            // Instantiate snowball at player's position facing the same direction as the player
//            _currentSnowBall = Instantiate(snowBallPrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
//            _currentSnowBall.transform.up = PlayerManager.instance.player.transform.up;
//            _projectileDirection = PlayerManager.instance.player.transform.up;
//            Debug.Log("Making Snow Ball...");

//            // Get its rigidbody component, and set its velocity to the direction it is facing multiplied by the projectile speed.
//            if (_currentSnowBall.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
//            {
//                rb.velocity = _currentSnowBall.transform.up * _projectileSpeed;
//            }
//            else
//            {
//                Debug.LogWarning("Projectile prefab needs a Rigidbody component for movement!");
//            }
        
//    }

//    public override void End()
//    {
//        // decrease player damage by buff amount
//        if (player != null)
//        {
//            if (_currentSnowBall)
//            {
//                Destroy(_currentSnowBall);
//            }
//            //PlayerManager.instance.RemoveAbility(this);
//        }
//        else
//        {

//        }
//    }
//}
