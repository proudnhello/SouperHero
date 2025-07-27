using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static FinishedSoup;
using UnityEngine.UI;

public class AbilityIconTooltip : MonoBehaviour, ITooltipSource
{
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] Image Icon;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;
    [SerializeField] Vector3 TooltipOffset;

    public void SetupTooltip(SoupAbility ability)
    {
        gameObject.SetActive(true);
        Tooltip.gameObject.SetActive(false);
        Tooltip.transform.localPosition = TooltipOffset;
        TooltipText.text = LocalizationManager.GetLocalizedString(ability.baseIngredient.IngredientName + " Profile");
        Icon.sprite = BioDatabase.Singleton.AbilityIcons[ability.ability];
    }
    public void OnHoverEnter()
    {
        if (IFadeAnim != null) StopCoroutine(IFadeAnim);
        StartCoroutine(IFadeAnim = FadeAnim(true));
    }

    public void OnHoverExit()
    {
        if (IFadeAnim != null) StopCoroutine(IFadeAnim);
        StartCoroutine(IFadeAnim = FadeAnim(false));
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
