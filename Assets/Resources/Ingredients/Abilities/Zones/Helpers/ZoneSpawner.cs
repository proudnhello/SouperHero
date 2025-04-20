// portions of this file were generated using GitHub Copilot
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/Helper/ZoneSpawner")]
public class ZoneSpawner : ScriptableObject
{
    public GameObject projectilePrefab;
    List<ZoneCore> projectilePool;
    private void OnEnable()
    {
        projectilePool = new();
    }

    public ZoneCore GetProjectile()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i] && projectilePool[i].gameObject && !projectilePool[i].gameObject.activeInHierarchy)
            {
                return projectilePool[i];
            }
        }
        ZoneCore proj = Instantiate(projectilePrefab).GetComponent<ZoneCore>();
        proj.gameObject.SetActive(false);
        projectilePool.Add(proj);
        return proj;
    }
}