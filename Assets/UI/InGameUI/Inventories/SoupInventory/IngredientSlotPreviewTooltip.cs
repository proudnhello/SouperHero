using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngredientSlotPreviewTooltip : MonoBehaviour, ITooltipSource
{
    [SerializeField] CanvasGroup Tooltip;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] TMP_Text TooltipText;
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] Vector2 BoxColliderMultiplier;
    [SerializeField] Vector3 TooltipOffset;
    [SerializeField] string TOOLTIP_KEY;
    [SerializeField] string TOOLTIP_KEY_TYPE;
    [SerializeField] bool isWildcard;
    [SerializeField] string TOOLTIP_INGREDIENT_SINGULAR;
    [SerializeField] string TOOLTIP_INGREDIENT_PLURAL;
    public Color SLOT_COLOR;

    public void SetupTooltip(Vector3 p1, Vector3 p2, int amount)
    {
        gameObject.SetActive(true);
        transform.position = (p1 + p2) / 2;
        boxCollider2D.size = new Vector2(Mathf.Abs(p2.x - p1.x) * BoxColliderMultiplier.x, Mathf.Abs(p2.y - p1.y) * BoxColliderMultiplier.y);
        Tooltip.gameObject.SetActive(false);
        Tooltip.transform.localPosition = new Vector3(Mathf.Abs(p2.x - p1.x)/ 2 * BoxColliderMultiplier.x, 0) + TooltipOffset;
        TooltipText.text = LocalizationManager.GetLocalizedString(TOOLTIP_KEY) + " " + amount + " ";
        string ing = amount > 1 ? LocalizationManager.GetLocalizedString(TOOLTIP_INGREDIENT_PLURAL) : LocalizationManager.GetLocalizedString(TOOLTIP_INGREDIENT_SINGULAR);
        if (isWildcard) TooltipText.text += ing + " " + LocalizationManager.GetLocalizedString(TOOLTIP_KEY_TYPE);
        else TooltipText.text += LocalizationManager.GetLocalizedString(TOOLTIP_KEY_TYPE) + " " + ing;
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
