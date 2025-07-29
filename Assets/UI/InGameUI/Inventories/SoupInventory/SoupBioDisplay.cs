using System;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class SoupBioDisplay : MonoBehaviour
{
    [SerializeField] RectTransform BioHolder;
    [SerializeField] GameObject SoupBaseSection;
    [SerializeField] GameObject FinishedSoupSection;
    [SerializeField] FlavorIconTextTooltip[] TextFlavorIconTooltips;
    [SerializeField] string SPACING_TEXT_FOR_ICON;

    [Header("Positions")]
    [SerializeField] TMP_Text TitleText;
    [SerializeField] Vector2 TitleTextPositions;
    [SerializeField] TMP_Text SoupDescriptionText;
    [SerializeField] Vector2 SoupDescriptionTextPositions;
    [SerializeField] StatTooltip CooldownStat;
    [SerializeField] Vector2 CooldownStatPositions;

    [Header("Slot Preview")]
    [SerializeField] Image[] IngSlotObjects;
    [SerializeField] IngredientSlotPreviewTooltip[] SlotPreviewTooltips;
    [SerializeField] float SlotSeparator = 55f;
    [SerializeField] Vector2 SlotSpriteHalfDimensions = new Vector2(25f, 27f);
    public Sprite[] IngredientSlotPreviewSprites;

    [Header("Finished Soup")]
    [SerializeField] AbilityIconTooltip[] AbilityIconObjects;
    [SerializeField] float IconSeparator = 75f;
    [SerializeField] FlavorBioTicks[] TickFlavorIcons;

    [Header("Bio Anim")]
    [SerializeField] BoxCollider2D HoverSpace;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float FadeAnimTime;
    [SerializeField] CanvasGroup BioFader;
    [SerializeField] float LeaveHoverSpaceDelay;

    SoupInventoryUI ui;
    public void Init(SoupInventoryUI ui)
    {
        this.ui = ui;
        BioHolder.gameObject.SetActive(false);
        foreach (var icon in TickFlavorIcons)
        {
            icon.Init();
        }
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
            if (PlayerEntityManager.Singleton.playerMovement.IsMoving() && !SoupInventoryUI.Singleton.IsOpen)
            {
                TriggerFadeAnim(false);
            }
        }
    }

    void TriggerFadeAnim(bool fadeIn, float delay = 0, ISoupBowl bowl = null)
    {
        // if fading out = null, if bowl is null = currSoup, if bowl is given = new bowl
        bowlInQueue = !fadeIn ? null : GetBase(bowl) ?? currSoup;

        // CONDITIONS
        // if bio is empty and no replacement, don't display
        if (bowl == null && currSoup == null) return;

        // if bio is fading in and new request is to fade in same bowl, don't display
        if (bowlInQueue != null && LastRequestWasFadeIn && fadeIn && currSoup == bowlInQueue) return;

        LastRequestWasFadeIn = fadeIn;
        if (IFadeBio != null) StopCoroutine(IFadeBio);
        StartCoroutine(IFadeBio = FadeBioAnim(fadeIn, delay, bowl));
    }

    IEnumerator IFadeBio;
    float fadeTimeProgressed;
    bool LastRequestWasFadeIn = false;
    SoupBase bowlInQueue;
    IEnumerator FadeBioAnim(bool fadeIn, float delay = 0, ISoupBowl bowl = null)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        bool displayNewSoup = bowlInQueue != currSoup;
        currSoup = bowlInQueue;

        BioHolder.gameObject.SetActive(true);
        // fade out (if already faded in)
        if (displayNewSoup)
        {
            while (fadeTimeProgressed >= 0)
            {
                var percentCompleted = Mathf.Clamp01(fadeTimeProgressed / FadeAnimTime);
                var curveAmount = FadeCurve.Evaluate(percentCompleted);
                BioFader.alpha = Mathf.Lerp(0, 1, curveAmount);

                yield return null;
                fadeTimeProgressed -= Time.deltaTime;
            }
        }
        
        // fade in (if chosen to)
        if (fadeIn)
        {
            if (bowl != null) ShowBio(bowl);
            while (fadeTimeProgressed <= FadeAnimTime)
            {
                var percentCompleted = Mathf.Clamp01(fadeTimeProgressed / FadeAnimTime);
                var curveAmount = FadeCurve.Evaluate(percentCompleted);
                BioFader.alpha = Mathf.Lerp(0, 1, curveAmount);

                yield return null;
                fadeTimeProgressed += Time.deltaTime;
            }
        }



        if (fadeIn) BioFader.alpha = 1;
        else
        {
            BioFader.gameObject.SetActive(false);
            currSoup = null;
        }

        fadeTimeProgressed = fadeIn ? FadeAnimTime : 0;
    }



    SoupBase currSoup;
    bool isDragging;
    public void DragBowl(ISoupBowl bowlInSlot)
    {
        TriggerFadeAnim(true, 0, bowlInSlot);
        isDragging = true;
    }

    public void ReleaseDrag()
    {
        isDragging = false;
    }
    public void TryDisplayHoverBio(ISoupBowl bowl)
    {
        if (isDragging) return;
        TriggerFadeAnim(true, 0, bowl);
    }
    public void TryHideHoverBio(ISoupBowl bowl)
    {
        if (isDragging || GetBase(bowl) != currSoup || IsTouchingHoverSpace) return;
        TriggerFadeAnim(false);
    }
    SoupBase GetBase(ISoupBowl bowl)
    {
        if (bowl is SoupBase soupBase) return soupBase;
        else if (bowl is FinishedSoup soup) return soup.soupBase;
        return null;
    }
    void ShowBio(ISoupBowl bowl)
    {
        if (bowl is SoupBase soupBase)
            ShowBaseBio(soupBase);
        else if (bowl is FinishedSoup finishedSoup)
            ShowFinishedSoupBio(finishedSoup);
    }


    void ShowBaseBio(SoupBase soup)
    {
        FinishedSoupSection.SetActive(false);
        SoupBaseSection.SetActive(true);
        TitleText.text = LocalizationManager.GetLocalizedString(soup.baseName);
        TitleText.transform.localPosition = new Vector2(TitleText.transform.localPosition.x, TitleTextPositions.x);

        SoupDescriptionText.transform.localPosition = new Vector2(SoupDescriptionText.transform.localPosition.x, SoupDescriptionTextPositions.x);
        ShowFlavorProfile(soup);

        CooldownStat.SetStat(soup.cooldown, Color.white);
        CooldownStat.transform.localPosition = new Vector2(CooldownStat.transform.localPosition.x, CooldownStatPositions.x);

        int numSlots = soup.maxAbilityIngredients + soup.maxFlavorIngredients + soup.maxWildcardIngredients;
        float evenNumCentererThing = numSlots % 2 == 0 ? SlotSeparator / 2 : 0; // if it's 2 or 4, then offset it back so it's centered
        float startPos = Mathf.FloorToInt(numSlots / 2)*-SlotSeparator + evenNumCentererThing;
        for (int i = 0; i < numSlots; i++)
        {
            IngSlotObjects[i].transform.localPosition = new Vector2(startPos + i * SlotSeparator, 0);
            IngSlotObjects[i].gameObject.SetActive(true);
        }
        for (int i = numSlots; i < IngSlotObjects.Length; i++) IngSlotObjects[i].gameObject.SetActive(false); // deactivate any remaining

        int IngSlot = 0;
        void IngSlotSetter(int soupIngAmount, int tooltip)
        {
            if (soupIngAmount > 0)
            {
                for (int i = IngSlot; i < IngSlot + soupIngAmount; i++)
                {
                    IngSlotObjects[i].color = SlotPreviewTooltips[tooltip].SLOT_COLOR;
                }
                Vector2 p1 = new(IngSlotObjects[IngSlot].transform.position.x - SlotSpriteHalfDimensions.x, IngSlotObjects[IngSlot].transform.position.y - SlotSpriteHalfDimensions.y);
                IngSlot += soupIngAmount-1;
                Vector2 p2 = new(IngSlotObjects[IngSlot].transform.position.x + SlotSpriteHalfDimensions.x, IngSlotObjects[IngSlot].transform.position.y + SlotSpriteHalfDimensions.y);
                SlotPreviewTooltips[tooltip].SetupTooltip(p1, p2, soupIngAmount);
                IngSlot++;
            }
            else SlotPreviewTooltips[tooltip].gameObject.SetActive(false);
        }

        IngSlotSetter(soup.maxAbilityIngredients, 0);
        IngSlotSetter(soup.maxFlavorIngredients, 1);
        IngSlotSetter(soup.maxWildcardIngredients, 2);
    }

    void ShowFinishedSoupBio(FinishedSoup soup)
    {
        FinishedSoupSection.SetActive(true);
        SoupBaseSection.SetActive(false);
        TitleText.text = LocalizationManager.GetLocalizedString(soup.soupBase.finishedSoupName);
        TitleText.transform.localPosition = new Vector2(TitleText.transform.localPosition.x, TitleTextPositions.y);

        SoupDescriptionText.transform.localPosition = new Vector2(SoupDescriptionText.transform.localPosition.x, SoupDescriptionTextPositions.y);
        ShowFlavorProfile(soup.soupBase);

        Color cooldownColor = soup.cooldown < soup.soupBase.cooldown ? BioDatabase.Singleton.BuffFlavorIcons[FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed].COLOR : Color.white;
        CooldownStat.SetStat(soup.cooldown, cooldownColor);
        CooldownStat.transform.localPosition = new Vector2(CooldownStat.transform.localPosition.x, CooldownStatPositions.y);

        int numSlots = soup.soupAbilities.Count;
        float evenNumCentererThing = numSlots % 2 == 0 ? IconSeparator / 2 : 0; // if it's 2 or 4, then offset it back so it's centered
        float startPos = Mathf.FloorToInt(numSlots / 2) * -IconSeparator + evenNumCentererThing;
        for (int i = 0; i < numSlots; i++)
        {
            AbilityIconObjects[i].transform.localPosition = new Vector2(startPos + i * IconSeparator, 0);
            AbilityIconObjects[i].SetupTooltip(soup.soupAbilities.Values.ToList()[i]);
        }
        for (int i = numSlots; i < AbilityIconObjects.Length; i++) AbilityIconObjects[i].gameObject.SetActive(false); // deactivate any remaining

        foreach (var icon in TickFlavorIcons) icon.Clear();
        int iconUsed = 0;
        foreach (var buff in soup.soupBuffStats.Values)
        {
            TickFlavorIcons[iconUsed].Set(buff);
            iconUsed++;
        }
        foreach (var inf in soup.soupInflictionStats.Values)
        {
            TickFlavorIcons[iconUsed].Set(inf);
            iconUsed++;
        }
    }

    void ShowFlavorProfile(SoupBase soup)
    {
        foreach (var icon in TextFlavorIconTooltips) icon.ClearIcons();

        // PARSE FLAVORS IN TEXT AND REPLACE WITH ICONS
        string localizedstr = LocalizationManager.GetLocalizedString(soup.finishedSoupName + " Profile");
        string[] words = localizedstr.Split(' ');

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
                    foreach (var buff in soup.inherentBuffFlavors)
                    {
                        if (buff.buffType == iconInfo.buffType)
                        {
                            iconCount = Mathf.RoundToInt(buff.amount);
                        }
                    }
                }
                else
                {
                    foreach (var infliction in soup.inherentInflictionFlavors)
                    {
                        if (infliction.inflictionType == iconInfo.inflictionType)
                        {
                            iconCount = Mathf.RoundToInt(infliction.amount);
                        }
                    }
                }
                display += "<alpha=#00>";
                for (int icon = 0; icon < iconCount; icon++)
                {
                    display += SPACING_TEXT_FOR_ICON;
                }
                display += "<alpha=#FF>" + "<color=#" + iconInfo.COLOR.ToHexString() + ">" + LocalizationManager.GetLocalizedString(word) + "<color=#FFFFFF>";
                SoupDescriptionText.text = display;
                SoupDescriptionText.ForceMeshUpdate();

                var p1Char = SoupDescriptionText.textInfo.characterInfo[SoupDescriptionText.textInfo.wordInfo[i].firstCharacterIndex];
                var p2Char = SoupDescriptionText.textInfo.characterInfo[SoupDescriptionText.textInfo.wordInfo[i].lastCharacterIndex];
                TextFlavorIconTooltips[iconToolTipTracker].SetBounds(
                    SoupDescriptionText.transform.TransformPoint(p1Char.bottomLeft),
                    SoupDescriptionText.transform.TransformPoint(p2Char.topRight)
                );

                TextFlavorIconTooltips[iconToolTipTracker].SetText(iconInfo);
                for (int icon = 0; icon < iconCount; icon++)
                {
                    var firstSpacingChar = SoupDescriptionText.textInfo.characterInfo[SoupDescriptionText.textInfo.wordInfo[i].firstCharacterIndex + icon * SPACING_TEXT_FOR_ICON.Length];
                    var spaceLocation = SoupDescriptionText.transform.TransformPoint((firstSpacingChar.topLeft + firstSpacingChar.bottomLeft) / 2f);
                    TextFlavorIconTooltips[iconToolTipTracker].SetIcon(iconInfo, spaceLocation);
                }
                iconToolTipTracker++;
            }
            else display += word;
            display += ' ';
        }

        SoupDescriptionText.text = display;
        SoupDescriptionText.ForceMeshUpdate();
    }

    public void OnCook(FinishedSoup newSoup)
    {
        if (currSoup == newSoup.soupBase)
        {
            TriggerFadeAnim(true, 0, newSoup);
        }
    }
}