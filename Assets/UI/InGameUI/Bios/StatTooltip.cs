using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatTooltip : MonoBehaviour, ITooltipSource
{
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] float colliderSizeMultiplier;
    [SerializeField] TMP_Text StatText;
    [SerializeField] string StatSuffix;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;
    [SerializeField] string TooltipLocalizationKEY;
    [SerializeField] string AmountStringDisplay = "F1";
    private void Start()
    {
        TooltipText.text = LocalizationManager.GetLocalizedString(TooltipLocalizationKEY);
        Tooltip.gameObject.SetActive(false);
    }

    public void Clear()
    {
        StatText.text = "";
    }

    public void SetColliderSize(float amount)
    {
        amount *= colliderSizeMultiplier;
        boxCollider2D.size = new Vector2(amount, boxCollider2D.size.y);
    }

    public void SetStat(float amount)
    {
        timeProgressed = 0;
        Tooltip.gameObject.SetActive(false);
        StatText.text = amount.ToString(AmountStringDisplay) + StatSuffix;
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
