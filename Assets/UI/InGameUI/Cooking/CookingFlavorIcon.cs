using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using FlavorIconInfo = BioDatabase.FlavorIconInfo;

public class CookingFlavorIcon : MonoBehaviour, ITooltipSource
{
    [SerializeField] Image icon;
    [SerializeField] Image[] tickObjects;
    [SerializeField] CookingFlavorTooltip tooltip;
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] float colliderDistanceMultiplier;
    [SerializeField] AnimationCurve remainderTickSizeCurve;
    int[] randomTicks;
    FlavorIconInfo currIcon;
    public void Init()
    {
        randomTicks = new int[CookingScreen.Singleton.cookingBioDisplay.tickMarkSprites.Length-1];
        gameObject.SetActive(false);
        tooltip.Init();
        tooltip.transform.SetParent(transform.parent);
    }
    public void Set(FinishedSoup.SoupBuffStat stat)
    {
        SetIcon(BioDatabase.Singleton.BuffFlavorIcons[stat.BuffType], stat.Amount);
    }
    public void Set(FinishedSoup.SoupInflictionStat stat)
    {
        SetIcon(BioDatabase.Singleton.InflictionFlavorIcons[stat.InflictionType], stat.Amount);
    }

    void SetIcon(FlavorIconInfo flavorInfo, float amount)
    {
        currIcon = flavorInfo;
        gameObject.SetActive(true);
        icon.sprite = flavorInfo.ICON;
        int seed = 0;
        foreach (var c in flavorInfo.KEY) seed += c;
        Random.InitState(seed);

        foreach (var tick in tickObjects) tick.gameObject.SetActive(false);

        int intAmount = Mathf.Clamp(Mathf.FloorToInt(amount), 0, 4);
        float remainder = amount % intAmount;
        if (remainder >= .1f) intAmount++;

        for (int t = 0; t < randomTicks.Length; t++) randomTicks[t] = t + 1;
        int selection = 0;
        for (int i = 0; i < intAmount; i++)
        {
            int index = Random.Range(0, randomTicks.Length - 1);
            (selection, randomTicks[index]) = (randomTicks[index], selection);
            tickObjects[i].gameObject.SetActive(true);
            tickObjects[i].sprite = CookingScreen.Singleton.cookingBioDisplay.tickMarkSprites[selection];
            tickObjects[i].color = flavorInfo.COLOR;
            tickObjects[i].transform.localScale = Vector3.one;
        }

        if (remainder >= .1f)
        {
            float size = remainderTickSizeCurve.Evaluate(remainder);
            tickObjects[intAmount - 1].transform.localScale = new Vector3(size, size, size);
        }

        float colSizeX = Mathf.Abs(transform.position.x - tickObjects[intAmount - 1].transform.position.x) * colliderDistanceMultiplier;
        boxCollider2D.size = new Vector2(colSizeX, boxCollider2D.size.y);
        boxCollider2D.offset = new Vector2(colSizeX / 2, 0);
    }

    public void Clear()
    {
        tooltip.EndAnim();
        gameObject.SetActive(false);
    }

    public async void OnHoverEnter()
    {
        while (tooltip.InProgress) await Task.Yield();
        tooltip.StartAnim();
        tooltip.SetText(LocalizationManager.GetLocalizedString(currIcon.KEY + " Tooltip"));
    }

    public void OnHoverExit()
    {
        tooltip.EndAnim();
    }
}