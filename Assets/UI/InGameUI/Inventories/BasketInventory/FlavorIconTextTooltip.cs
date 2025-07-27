using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Icon = BioDatabase.FlavorIconInfo;
using TMPro;

public class FlavorIconTextTooltip : MonoBehaviour, ITooltipSource
{
    [SerializeField] Image[] FlavorIcons;
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] Vector2 colliderDistMultiplier;
    [SerializeField] Vector3 TooltipOffset;
    [SerializeField] bool TooltipOnRight = false;
 
    int usedIcons;
    public void ClearIcons()
    {
        foreach (var icon in FlavorIcons)
        {
            icon.gameObject.SetActive(false);
        }
        Tooltip.gameObject.SetActive(false);
        usedIcons = 0;
        boxCollider2D.enabled = false;
    }
    public void SetText(Icon icon)
    {
        TooltipText.text = LocalizationManager.GetLocalizedString(icon.KEY + " Tooltip");       // get the localized version of the tooltip text using the key
    }
    public void SetIcon(Icon icon, Vector3 pos)
    {
        FlavorIcons[usedIcons].sprite = icon.ICON;
        FlavorIcons[usedIcons].transform.position = pos;
        FlavorIcons[usedIcons].gameObject.SetActive(true);
        usedIcons++;
    }
    Vector3 farRightTextPoint;
    public void SetBounds(Vector3 p1, Vector3 p2)
    {
        transform.position = (p1 + p2) / 2;
        boxCollider2D.size = new Vector2(Mathf.Abs(p2.x - p1.x), Mathf.Abs(p2.y - p1.y)) * colliderDistMultiplier;
        farRightTextPoint = p2;
        boxCollider2D.enabled = true;
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
        if (TooltipOnRight)
        {
            Tooltip.transform.localPosition = farRightTextPoint + TooltipOffset;
        }
        else
        {
            Tooltip.transform.localPosition = FlavorIcons[0].transform.localPosition + TooltipOffset;
        }

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