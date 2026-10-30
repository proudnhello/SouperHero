/*
 * This file was modified with the help of LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log10.md
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownBar : MonoBehaviour
{
    public Image cooldownFill;
    [SerializeField] GameObject cooldownHolder;

    void Update()
    {
        ISoupBowl bowl = PlayerInventory.Singleton.GetCurrentBowl();
        if (bowl is FinishedSoup soup)
        {
            cooldownFill.enabled = true;
            cooldownFill.fillAmount = soup.GetCooldownPercentage();
        } 
        else
        {
            cooldownFill.enabled = false;
        }
    }

    private void OnEnable()
    {
        GameManager.OnEndRun += GameEnd;
    }

    void GameEnd(bool s)
    {
        cooldownHolder.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.OnEndRun -= GameEnd;
    }
}
