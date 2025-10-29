using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthbarManager : MonoBehaviour
{
    [SerializeField] EnemyHealthBar bossHealthBar;
    public static BossHealthbarManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartBossFight(EnemyBaseClass boss)
    {
        bossHealthBar.gameObject.SetActive(true);
        bossHealthBar.SetEnemy(boss);
    }

    public void EndBossFight()
    {
        bossHealthBar.gameObject.SetActive(false);
    }
}
