using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FinishedSoup;
using FlavorIconInfo = BioDatabase.FlavorIconInfo;

public class FlavorBioTicks : MonoBehaviour, ITooltipSource
{
    [SerializeField] Image icon;
    [SerializeField] Image[] tickObjects;
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] float colliderDistanceMultiplier;
    [SerializeField] AnimationCurve remainderTickSizeCurve;
    [SerializeField] Color CarryoverColor;

    [Header("Tooltip")]
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;

    int[] randomTicks;
    FlavorIconInfo currIcon;
    public void Init()
    {
        randomTicks = new int[CookingScreen.Singleton.cookingBioDisplay.tickMarkSprites.Length-1];
        Clear();
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        Tooltip.gameObject.SetActive(false);
    }
    public void Set(SoupBuffStat stat)
    {
        SetIcon(BioDatabase.Singleton.BuffFlavorIcons[stat.BuffType], stat.Amount);
    }
    public void Set(SoupInflictionStat stat)
    {
        SetIcon(BioDatabase.Singleton.InflictionFlavorIcons[stat.InflictionType], stat.Amount);
    }

    void SetIcon(FlavorIconInfo flavorInfo, float amount)
    {
        currIcon = flavorInfo;
        gameObject.SetActive(true);
        TooltipText.text = LocalizationManager.GetLocalizedString(currIcon.KEY + " Tooltip");
        icon.sprite = flavorInfo.ICON;
        int seed = 0;
        foreach (var c in flavorInfo.KEY) seed += c;
        Debug.Log("why am i here");
        Random.InitState(seed);

        foreach (var tick in tickObjects) tick.gameObject.SetActive(false);

        int intAmount = Mathf.Clamp(Mathf.FloorToInt(amount), 0, 9);
        float remainder = amount % intAmount;
        if (remainder >= .1f) intAmount++;

        int selection = 0;
        for (int i = 0; i < intAmount; i++)
        {
            if (i == 0 || i == 5)
            {
                for (int t = 0; t < randomTicks.Length; t++) randomTicks[t] = t + 1;
                selection = 0;
                Random.InitState(seed); // reset for next yellow wave
            }
            int index = Random.Range(0, randomTicks.Length - 1);
            (selection, randomTicks[index]) = (randomTicks[index], selection);
            tickObjects[i].gameObject.SetActive(true);
            tickObjects[i].sprite = CookingScreen.Singleton.cookingBioDisplay.tickMarkSprites[selection];
            tickObjects[i].color = i < 5 ? flavorInfo.COLOR : CarryoverColor;
            tickObjects[i].transform.localScale = Vector3.one;
        }

        if (remainder >= .1f)
        {
            float size = remainderTickSizeCurve.Evaluate(remainder);
            tickObjects[intAmount - 1].transform.localScale = new Vector3(size, size, size);
        }

        float colSizeX = Mathf.Abs(transform.position.x - tickObjects[Mathf.Min(intAmount - 1, 4)].transform.position.x) * colliderDistanceMultiplier;
        boxCollider2D.size = new Vector2(colSizeX, boxCollider2D.size.y);
        boxCollider2D.offset = new Vector2(colSizeX / 2, 0);
    }

    public void OnHoverEnter()
    {
        if (IFadeAnim != null) StopCoroutine(IFadeAnim);
        StartCoroutine(IFadeAnim = FadeAnim(true));
    }

    public void OnHoverExit()
    {
        if (IFadeAnim != null) StopCoroutine(IFadeAnim);
        if (gameObject.activeInHierarchy) StartCoroutine(IFadeAnim = FadeAnim(false));
    }

    float timeProgressed = 0;
    IEnumerator IFadeAnim;
    IEnumerator FadeAnim(bool fadeIn)
    {
        Tooltip.gameObject.SetActive(true);

        while (timeProgressed >= 0 && timeProgressed <= FadeAnimTime)
        {
            var percentCompleted = Mathf.Clamp01(timeProgressed / FadeAnimTime);
            var curveAmount = FadeCurve.Evaluate(percentCompleted);
            Tooltip.alpha = Mathf.Lerp(0, 1, curveAmount);

            yield return null;
            timeProgressed = fadeIn ? timeProgressed + Time.deltaTime : timeProgressed - Time.deltaTime;
        }

        if (fadeIn) Tooltip.alpha = 1;
        else Tooltip.gameObject.SetActive(false);

        timeProgressed = fadeIn ? FadeAnimTime : 0;
    }
}