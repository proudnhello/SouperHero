using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthbarManager : MonoBehaviour
{
    [SerializeField] EnemyHealthBar bossHealthBar;
    [SerializeField] float healthBarFillTime = 2f;
    public static BossHealthbarManager Instance { get; private set; }

    [SerializeField] FMODUnity.EventReference healthBarAppearSFX;

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

    public IEnumerator StartBossFight(BossAI boss)
    {
        bossHealthBar.gameObject.SetActive(true);
        bossHealthBar.GetComponent<EnemyHealthBar>().enabled = false;

        float timer = 0f;
        Slider slider = bossHealthBar.GetComponentInChildren<Slider>();
        FMODUnity.RuntimeManager.PlayOneShot(healthBarAppearSFX, boss.transform.position);
        while (timer < healthBarFillTime)
        {
            timer += Time.deltaTime;
            float fillRatio = timer / healthBarFillTime;
            fillRatio = fillRatio * fillRatio * fillRatio * (fillRatio * (6f * fillRatio - 15f) + 10f); // smootherstep, because smoothstep wasn't really noticeable
            slider.value = fillRatio;
            yield return null;
        }

        bossHealthBar.GetComponent<EnemyHealthBar>().enabled = true;
        bossHealthBar.SetEnemy(boss);
        boss.holdSpawning = false;
    }

    public void EndBossFight()
    {
        bossHealthBar.gameObject.SetActive(false);
    }
}
