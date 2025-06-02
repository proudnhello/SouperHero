using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Icon = Encyclopedia.FlavorTextToIcon;
using TMPro;

public class EncyclopediaFlavorIcon : MonoBehaviour
{
    [SerializeField] Image FlavorIconDisplay;
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;      // this just holds a key now
                                                // key references the actual text we want to display in the tooltip

    public void SetIcon(Icon icon)
    {
        FlavorIconDisplay.sprite = icon.ICON;
        TooltipText.text = LocalizationManager.GetLocalizedString(icon.TOOLTIP_TEXT);       // get the localized version of the tooltip text using the key
        gameObject.SetActive(true);
        Tooltip.gameObject.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (IFadeAnim != null) StopCoroutine(IFadeAnim);
        StartCoroutine(IFadeAnim = FadeAnim(true));
    }

    private void OnMouseExit()
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