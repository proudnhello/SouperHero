using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Abilities/MineAttack")]

//public class MineAttack : AbilityAbstractClass
//{
//    private PlayerManager player;
//    [SerializeField] GameObject minePrefab;
//    [SerializeField] private int damageMult = 2;
//    //[SerializeField] private float delayMult = 3;
//    [SerializeField] private float sizeMult = 1.5f;
//    [SerializeField] private float mineLifespan = 5f;

//    [SerializeField] private float explosionTime = 0.5f;
//    [SerializeField] private float explosionRadiusMult = 2f;

//    private GameObject currentMine;

//    public override void Initialize(int soupVal)
//    {
//        Debug.Log("MineAttack initialized");
//    }

//    public override void UseAbility()
//    {

//        if (currentMine == null)
//        {
//            currentMine = Instantiate(minePrefab, PlayerManager.instance.player.transform.position, Quaternion.identity);
//            currentMine.transform.position = PlayerManager.instance.player.transform.position;
//            currentMine.transform.localScale = new Vector3(sizeMult, sizeMult, sizeMult);
//        }

//        MineDamage mine = currentMine.GetComponent<MineDamage>();
//        mine.despawnTime = mineLifespan;
//        mine.explosionTime = explosionTime;
//        mine.explosionRadius = PlayerManager.instance.GetAttackRadius() * explosionRadiusMult;
//        mine.damageMult = damageMult;

//        Debug.Log("MineAttack active");
//    }

//    public override void End()
//    {
//        Debug.Log("MineAttack ended");
//        //PlayerManager.instance.RemoveAbility(this);
//    }

//}
