using TMPro;
using UnityEngine;
using System.Collections;

public class CookingFlavorTooltip : MonoBehaviour
{
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;

    public void Init()
    {
        gameObject.SetActive(true);
        Clear();
    }
    public void SetText(string text)
    {
        TooltipText.text = text;
    }

    public void StartAnim()
    {
        StopAllCoroutines();
        StartCoroutine(IFadeAnim = FadeAnim(true));
    }

    public void EndAnim()
    {
        StopAllCoroutines();
        StartCoroutine(IFadeAnim = FadeAnim(false));
    }

    public void Clear()
    {
        Tooltip.gameObject.SetActive(false);
        Tooltip.alpha = 0;
        timeProgressed = 0;
    }

    public bool InProgress
    {
        get => IFadeAnim != null;
    }
    float timeProgressed = 0;
    internal IEnumerator IFadeAnim;
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
        else
        {
            IFadeAnim = null;
            Tooltip.gameObject.SetActive(false);
        }

        timeProgressed = fadeIn ? FadeAnimTime : 0;
    }
}