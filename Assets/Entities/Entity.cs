// portions of this file were generated using GitHub Copilot
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static EntityInflictionEffectHandler;
using static FlavorIngredient;
using static UnityEngine.ParticleSystem;
using Infliction = FinishedSoup.SoupInflictionStat;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;


public abstract class Entity : MonoBehaviour
{
    // ~~~ DEFINITIONS ~~~
    [Serializable]
    public struct BaseStats
    {
        public int maxHealth;
        public float baseMoveSpeed;
        public float invincibility;
    }
    [Serializable]
    public struct CurrentStats
    {
        public int health;
        //public float moveSpeed;
    }

    public SpriteMask submergeMask;
    // Bounds are used for collision detection with hazards. All EntityBounds should be children of the entity
    private EntityBounds[] bounds;
    [SerializeField] bool needsBounds = true;

    // ~~~ VARIABLES ~~~
    [SerializeField] BaseStats baseStats;
    [SerializeField] CurrentStats currentStats;
    internal EntityInflictionEffectHandler inflictionHandler;
    internal EntityRenderer entityRenderer;
    internal Rigidbody2D _rigidbody;
    public bool falling = false;
    public bool flying = false;
    // Counts reasons why the entity cannot attack
    // If > 0, the entity cannot attack
    // So, once a reason is removed, it should be decremented, but if there are multiple reasons, the entity will continue to be unable to attack
    private int cantAttack = 0;


    [SerializeField] GameObject hitmarker;

    public void InitEntity()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        inflictionHandler = new(this);
        ResetStats();
        bounds = GetComponentsInChildren<EntityBounds>();
        if (bounds.Length == 0 && needsBounds)
        {
            Debug.LogWarning("Entity " + gameObject.name +  "has no bounds, and will not be effected by hazards");
        }
    }

    public bool CanAttack()
    {
        return cantAttack <= 0;
    }   

    public void AddCantAttack()
    {
        cantAttack++;
    }

    // Used for hazard collision detection
    // If not all bounds are in the hazard, remove the entity from the hazard, otherwise add it
    public void UpdateBounds(Hazard hazard)
    {
        if (CheckBounds(hazard))
        {
            hazard.AddEntity(this);
        }
        else
        {
            hazard.RemoveEntity(this);
        }
    }

    public bool CheckBounds(Hazard hazard)
    {
        if (bounds.Length == 0)
        {
            return false;
        }
        foreach (EntityBounds bound in bounds)
        {
            if (!bound.CheckHazard(hazard))
            {
                return false;
            }
        }
        return true;
    }

    public void RemoveCantAttack()
    {
        cantAttack--;
        if (cantAttack < 0)
        {
            cantAttack = 0;
        }
    }

    protected bool invincible = false;
    public virtual bool IsInvincible() => invincible;

    public void ResetStats()
    {
        currentStats.health = baseStats.maxHealth;
        speedMults.Clear();
    }

    // Quiet makes it so no sound or hitmarker is played. Used currently for ground hazards
    public virtual void ApplyInfliction(List<Infliction> spoonInflictions, Transform source)
    {
        inflictionHandler.ApplyInflictions(spoonInflictions, source);
    }

    public bool HasInfliction(InflictionType infliction)
    {
        return inflictionHandler.HasInfliction(infliction);
    }

    // Displays hitmarkers
    public void DisplayHitmarker(InflictionType type, float amount)
    {
        GameObject hitmarkerInstance = Instantiate(hitmarker, transform.position, Quaternion.identity);
        ParticleSystem particleSystem = hitmarkerInstance.transform.GetComponentInChildren<ParticleSystem>(); //Access child particle game object on UI layer
        List<Sprite> particles = new List<Sprite>();

        for (int i = 0; i < particleSystem.textureSheetAnimation.spriteCount; i++)
        {
            particleSystem.textureSheetAnimation.RemoveSprite(i);
        }

        particles.Add(BioDatabase.Singleton.InflictionFlavorIcons[type].ICON);


        //If no particles, do not enable particle effects
        if (particleSystem.textureSheetAnimation.mode == 0) return;

        //Add all particle icons and activate particle system for slot
        foreach (var particle in particles)
        {
            if (particle == null) return; // If there is a null particle, do not enable particle effects
            particleSystem.textureSheetAnimation.AddSprite(particle);
        }
        if (particles.Count == 0) return; //If no particles, do not enable particle effects
        particleSystem.Play();

        Debug.Log(type + " displayed");

        //hitmarkerInstance.GetComponentInChildren<TextMeshPro>().text = amount + " " + FlavorIngredient.GetFlavorHitmarkerText(type);
        //hitmarkerInstance.GetComponentInChildren<TextMeshPro>().color = FlavorIngredient.GetFlavorHitmarkerColor(type);
    }

    public BaseStats GetBaseStats()
    {
        return baseStats;
    }

    public int GetHealth()
    {
        return currentStats.health;
    }

    // Directly edit the health of the entity, will not trigger damage effects
    public virtual void ModifyHealth(int amount)
    {
        currentStats.health += amount;
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }

    // Deal damage to the entity. Use this to trigger damage effects
    public virtual void DealDamage(int damage)
    {
        inflictionHandler.DealDamage(damage);
    }

    // Directly set the health of the entity, will not trigger damage effects
    public virtual void SetHealth(int health)
    {
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }

    public bool IsDead()
    {
        return currentStats.health <= 0;
    }

    Dictionary<int, float> speedMults = new();
    public float GetMoveSpeed()
    {
        float speed = baseStats.baseMoveSpeed;
        foreach (var mult in speedMults.Values) speed *= mult;
        if (speed < 1)
        {
            return 1f;
        }
        return speed;
    }
    public virtual void SetMoveSpeed(int sourceID, float speedMult)
    {
        if (!speedMults.TryAdd(sourceID, speedMult))
        {
            speedMults[sourceID] = speedMult;
        }
    }

    public virtual void ResetMoveSpeed(int sourceID)
    {
        speedMults.Remove(sourceID);
    }

    public float GetInvincibility()
    {
        return baseStats.invincibility;
    }

    // Default fall function calls for instant death. Player will overwrite this
    public abstract void Fall(Transform respawnPoint);
}
