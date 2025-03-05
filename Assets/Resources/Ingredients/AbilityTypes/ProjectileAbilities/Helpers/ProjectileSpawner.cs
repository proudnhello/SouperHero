using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/Helper/ProjectileSpawner")]
public class ProjectileSpawner : ScriptableObject
{
    public GameObject projectilePrefab;
    List<ProjectileObject> projectilePool;
    private void OnEnable()
    {
        projectilePool = new();
    }

    public ProjectileObject GetProjectile()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i] && projectilePool[i].gameObject && !projectilePool[i].gameObject.activeInHierarchy)
            {
                return projectilePool[i];
            }
        }
        ProjectileObject proj = Instantiate(projectilePrefab).GetComponent<ProjectileObject>();
        proj.gameObject.SetActive(false);
        projectilePool.Add(proj);
        return proj;
    }

    public void ClearPool()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            Destroy(projectilePool[i].gameObject);
        }
        projectilePool.Clear();
    }
}