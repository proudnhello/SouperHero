using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class ProjectileSpawner : ScriptableObject
{
    public GameObject projectilePrefab;
    List<ProjectileObject> projectilePool = new();

    public ProjectileObject GetProjectile()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (!projectilePool[i].gameObject.activeInHierarchy)
            {
                return projectilePool[i];
            }
        }

        ProjectileObject proj = Instantiate(projectilePrefab).GetComponent<ProjectileObject>();
        proj.gameObject.SetActive(false);
        projectilePool.Add(proj);
        return proj;
    }
}