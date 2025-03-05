using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Helper/VFX")]
public class AbilityVFXSpawner : ScriptableObject
{
    public GameObject meleeVFX;
    List<SpriteRenderer> meleeVFXPool;
    private void OnEnable()
    {
        meleeVFXPool = new();
    }

    public SpriteRenderer GetMelee(Transform parent)
    {
        for (int i = 0; i < meleeVFXPool.Count; i++)
        {
            if (meleeVFXPool[i] && meleeVFXPool[i].gameObject && !meleeVFXPool[i].gameObject.activeInHierarchy)
            {
                return meleeVFXPool[i];
            }
        }
        SpriteRenderer melee = Instantiate(meleeVFX, parent).GetComponentInChildren<SpriteRenderer>();
        meleeVFXPool.Add(melee);
        return melee;
    }

    public void ClearPools()
    {
        for (int i = 0; i < meleeVFXPool.Count; i++)
        {
            Destroy(meleeVFXPool[i].gameObject);
        }
        meleeVFXPool.Clear();
    }
}