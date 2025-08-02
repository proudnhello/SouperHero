using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

using System;
using Unity.VisualScripting;
using DG.Tweening.Core.Easing;
public class IngredientBioDisplay : MonoBehaviour
{

    public static IngredientBioDisplay Singleton { get; private set; }

    [Header("Links")]
    [SerializeField] CanvasGroup BioHolder;
    [SerializeField] TMP_Text Title;
    [SerializeField] TMP_Text IngredientTypeHeader;
    [SerializeField] GameObject FlavorSection;
    [SerializeField] TMP_Text FlavorEntry;
    [SerializeField] FlavorIconTextTooltip[] FlavorIconTooltips;
    [SerializeField] GameObject AbilitySection;
    [SerializeField] TMP_Text AbilityEntry;
    [SerializeField] Image AbilityIcon;
    [SerializeField] StatTooltip DamageStat;
    [SerializeField] StatTooltip DurationStat;

    [Header("Values")]
    [SerializeField] string SPACING_TEXT_FOR_ICON;
    [SerializeField] Color FlavorHeaderColor;
    [SerializeField] Color AbilityHeaderColor;

    [Header("Fade Anim")]
    [SerializeField] BoxCollider2D HoverSpace;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] float LeaveHoverSpaceDelay;
    public float HoverTimeToDisplay;
    public float UnhoverTimeToHide;


    void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;

        BioHolder.gameObject.SetActive(false);
    }

    bool IsTouchingHoverSpace = false;
    private void Update()
    {
        if (BioHolder.gameObject.activeInHierarchy)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool touching = HoverSpace.bounds.IntersectRay(ray);
            if (IsTouchingHoverSpace && !touching && !isDragging) // exit out hover space
            {
                TriggerFadeAnim(false, LeaveHoverSpaceDelay);
            }
            else if (!IsTouchingHoverSpace && touching && !isDragging) // bio is fading out, but you reenter hover space
            {
                TriggerFadeAnim(true);
            }
            IsTouchingHoverSpace = touching;
            if (PlayerEntityManager.Singleton.playerMovement.IsMoving() && !CursorManager.Singleton.IsHoldingSomething)
            {
                TriggerFadeAnim(false);
            }
        }
    }

    bool isDragging;
    Ingredient currIngredient;
    public void DragIngredient(Ingredient ing)
    {
        TriggerFadeAnim(true, 0, ing);
        isDragging = true;
    }

    public void ReleaseDrag()
    {
        isDragging = false;
    }

    public void TryDisplayHoverBio(Ingredient ing)
    {
        if (isDragging || ing == null) return;
        TriggerFadeAnim(true, 0, ing);
    }
    public void TryHideHoverBio(Ingredient ing)
    {
        if (isDragging || ing == null || ing != currIngredient || IsTouchingHoverSpace) return;
        TriggerFadeAnim(false, 0, ing);
    }

    void TriggerFadeAnim(bool fadeIn, float delay = 0, Ingredient ing = null)
    {
        if (IFadeBio != null) StopCoroutine(IFadeBio);
        StartCoroutine(IFadeBio = FadeBioAnim(fadeIn, delay, ing));
    }

    IEnumerator IFadeBio;
    float fadeTimeProgressed = 0;
    IEnumerator FadeBioAnim(bool fadeIn, float delay = 0, Ingredient ing = null)
    {

        if (delay > 0) yield return new WaitForSeconds(delay);

        BioHolder.gameObject.SetActive(true);
        if (ing != null)
        {
            currIngredient = ing;
            PullUpBio(currIngredient);
        }
        // fade out (if already faded in)
        while (fadeTimeProgressed >= 0 && fadeTimeProgressed <= FadeAnimTime)
        {
            var percentCompleted = Mathf.Clamp01(fadeTimeProgressed / FadeAnimTime);
            var curveAmount = FadeCurve.Evaluate(percentCompleted);
            BioHolder.alpha = Mathf.Lerp(0, 1, curveAmount);

            yield return null;
            fadeTimeProgressed = fadeIn ? fadeTimeProgressed + Time.deltaTime : fadeTimeProgressed - Time.deltaTime;
        }


        if (fadeIn) BioHolder.alpha = 1;
        else
        {
            BioHolder.gameObject.SetActive(false);
            currIngredient = null;
        }

        fadeTimeProgressed = fadeIn ? FadeAnimTime : 0;
    }

    void PullUpBio(Ingredient ing)
    {

        Title.text = LocalizationManager.GetLocalizedString(ing.IngredientName);    // Localize ingredient name

        if (ing.GetType() == typeof(FlavorIngredient))
        {
            AbilitySection.SetActive(false);
            FlavorSection.SetActive(true);

            foreach (var icon in FlavorIconTooltips) icon.ClearIcons();

            IngredientTypeHeader.text = LocalizationManager.GetLocalizedString("Flavor"); // LOCALIZE
            IngredientTypeHeader.color = FlavorHeaderColor;

            // PARSE FLAVORS IN TEXT AND REPLACE WITH ICONS
            string localizedstr = LocalizationManager.GetLocalizedString(ing.IngredientName + " Profile");    // Localize flavor profile
            string[] words = localizedstr.Split(' '); 

            // KNOWN REALLY ANNOYING THING TO KEEP IN MIND !!!!!!!!!!!!!!!!!!!
            //  if you ever have a newline between lines in an ingredient's bio MAKE SURE TO PUT IN A SPACE
            //  since it's the separator or it'll consider it one word!

            string display = "";
            int iconToolTipTracker = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (BioDatabase.Singleton.FlavorIcons.TryGetValue(word, out var iconInfo))
                {
                    int iconCount = 0;
                    if (iconInfo.isBuffType)
                    {
                        if (((FlavorIngredient)ing).Pairing.isBuff && ((FlavorIngredient)ing).Pairing.GetPairing() == (int)iconInfo.buffType) iconCount = 1;
                        else
                        {
                            foreach (var buff in ((FlavorIngredient)ing).buffFlavors)
                            {
                                if (buff.buffType == iconInfo.buffType)
                                {
                                    iconCount = Mathf.RoundToInt(buff.amount);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!((FlavorIngredient)ing).Pairing.isBuff && ((FlavorIngredient)ing).Pairing.GetPairing() == (int)iconInfo.inflictionType) iconCount = 1;
                        else
                        {
                            foreach (var infliction in ((FlavorIngredient)ing).inflictionFlavors)
                            {
                                if (infliction.inflictionType == iconInfo.inflictionType)
                                {
                                    iconCount = Mathf.RoundToInt(infliction.amount);
                                }
                            }
                        }
                    }
                    display += "<alpha=#00>";
                    for (int icon = 0; icon < iconCount; icon++)
                    {
                        display += SPACING_TEXT_FOR_ICON;
                    }
                    display += "<alpha=#FF>" + "<color=#" + iconInfo.COLOR.ToHexString() + ">" + LocalizationManager.GetLocalizedString(word) + "<color=#FFFFFF>";
                    FlavorEntry.text = display;
                    FlavorEntry.ForceMeshUpdate();

                    var p1Char = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[i].firstCharacterIndex];
                    var p2Char = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[i].lastCharacterIndex];
                    FlavorIconTooltips[iconToolTipTracker].SetBounds(
                        FlavorEntry.transform.TransformPoint(p1Char.bottomLeft),
                        FlavorEntry.transform.TransformPoint(p2Char.topRight)
                    );

                    FlavorIconTooltips[iconToolTipTracker].SetText(iconInfo);
                    for (int icon = 0; icon < iconCount; icon++)
                    {
                        var firstSpacingChar = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[i].firstCharacterIndex + icon * SPACING_TEXT_FOR_ICON.Length];
                        var spaceLocation = FlavorEntry.transform.TransformPoint((firstSpacingChar.topLeft + firstSpacingChar.bottomLeft) / 2f);
                        FlavorIconTooltips[iconToolTipTracker].SetIcon(iconInfo, spaceLocation);
                    }
                    iconToolTipTracker++;
                }
                else display += word;
                display += ' ';
            }

            FlavorEntry.text = display;
            FlavorEntry.ForceMeshUpdate();

        } 
        else // is AbilityIngredient
        {
            AbilitySection.SetActive(true);
            FlavorSection.SetActive(false);

            IngredientTypeHeader.text = LocalizationManager.GetLocalizedString("Ability"); // LOCALIZE
            IngredientTypeHeader.color = AbilityHeaderColor;

            AbilityEntry.text = LocalizationManager.GetLocalizedString(ing.IngredientName + " Profile");   // Localize ability description

            AbilityIcon.sprite = BioDatabase.Singleton.AbilityIcons[((AbilityIngredient)ing).abilityType];

            int dmg = 0;
            foreach (var infliction in ((AbilityIngredient)ing).inherentInflictionFlavors) 
                if (infliction.inflictionType == FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage)
                {
                    dmg = infliction.amount;
                    break;
                }
            DamageStat.SetStat(dmg, Color.white);

            DurationStat.SetStat(((AbilityIngredient)ing).baseStats.BaseDuration, Color.white);
        }
    }
}
