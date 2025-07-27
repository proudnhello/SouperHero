using System;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoupBioDisplay : MonoBehaviour
{
    [SerializeField] RectTransform BioHolder;
    [SerializeField] GameObject SoupBaseSection;
    [SerializeField] GameObject FinishedSoupSection;
    [SerializeField] FlavorIconTextTooltip[] FlavorIconTooltips;
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


    SoupInventoryUI ui;
    public void Init(SoupInventoryUI ui)
    {
        this.ui = ui;
        BioHolder.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CursorManager.CursorClickOut -= UnlockSlot;
    }

    private void Update()
    {
        if (BioHolder.gameObject.activeInHierarchy)
        {
            if (PlayerEntityManager.Singleton.playerMovement.IsMoving() && !SoupInventoryUI.Singleton.IsOpen)
            {
                BioHolder.gameObject.SetActive(false);
            }
        }
    }

    SoupBase currLockedSoup;
    SoupBase currSoup;
    bool isDragging;
    public void DragBowl(ISoupBowl bowlInSlot)
    {
        ShowBio(bowlInSlot);
        isDragging = true;
    }

    public void ReleaseDrag(ISoupBowl soupInReleasedSlot, bool tap)
    {
        isDragging = false;
        if (tap)
        {
            SoupBase bowl = GetBase(soupInReleasedSlot);
            if (bowl == currLockedSoup)
            {
                UnlockSlot();
            }
            else // Lock slot
            {
                currLockedSoup = bowl;
                CursorManager.CursorClickOut += UnlockSlot;
            }
        }
    }
    public void TryDisplayHoverBio(ISoupBowl bowl)
    {
        if (isDragging || currLockedSoup != null) return;
        ShowBio(bowl);
    }
    public void TryHideHoverBio(ISoupBowl bowl)
    {
        if (isDragging || currLockedSoup != null || GetBase(bowl) != currSoup) return;
        if (CookingScreen.Singleton.BowlCookingSlot.soupBaseReference != null)
            ShowBio(CookingScreen.Singleton.BowlCookingSlot.soupBaseReference);
        else CloseBio();
    }
    public void UnlockSlot()
    {
        isDragging = false;
        currLockedSoup = null;
        CursorManager.CursorClickOut -= UnlockSlot;
    }
    SoupBase GetBase(ISoupBowl bowl)
    {
        if (bowl is SoupBase soupBase) return soupBase;
        else return ((FinishedSoup)bowl).soupBase;
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
        if (currSoup == soup) return;
        currSoup = soup;

        BioHolder.gameObject.SetActive(true);
        FinishedSoupSection.SetActive(false);
        SoupBaseSection.SetActive(true);
        TitleText.text = LocalizationManager.GetLocalizedString(soup.baseName);
        TitleText.transform.localPosition = new Vector2(TitleText.transform.localPosition.x, TitleTextPositions.x);

        ShowFlavorProfile(soup);
        SoupDescriptionText.transform.localPosition = new Vector2(SoupDescriptionText.transform.localPosition.x, SoupDescriptionTextPositions.x);

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

    void ShowFinishedSoupBio(FinishedSoup soup, bool overrideShow = false)
    {
        if (currSoup == soup.soupBase && !overrideShow) return;
        currSoup = soup.soupBase;

        BioHolder.gameObject.SetActive(true);
        FinishedSoupSection.SetActive(true);
        SoupBaseSection.SetActive(false);
        TitleText.text = LocalizationManager.GetLocalizedString(soup.soupBase.finishedSoupName);
        TitleText.transform.localPosition = new Vector2(TitleText.transform.localPosition.x, TitleTextPositions.y);

        ShowFlavorProfile(soup.soupBase);
        SoupDescriptionText.transform.localPosition = new Vector2(SoupDescriptionText.transform.localPosition.x, SoupDescriptionTextPositions.y);

        Color cooldownColor = soup.cooldown < soup.soupBase.cooldown ? BioDatabase.Singleton.BuffFlavorIcons[FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed].COLOR : Color.white;
        CooldownStat.SetStat(soup.cooldown, cooldownColor);
        CooldownStat.transform.localPosition = new Vector2(CooldownStat.transform.localPosition.x, CooldownStatPositions.y);
    }

    void ShowFlavorProfile(SoupBase soup)
    {
        foreach (var icon in FlavorIconTooltips) icon.ClearIcons();

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
                FlavorIconTooltips[iconToolTipTracker].SetBounds(
                    SoupDescriptionText.transform.TransformPoint(p1Char.bottomLeft),
                    SoupDescriptionText.transform.TransformPoint(p2Char.topRight)
                );

                FlavorIconTooltips[iconToolTipTracker].SetText(iconInfo);
                for (int icon = 0; icon < iconCount; icon++)
                {
                    var firstSpacingChar = SoupDescriptionText.textInfo.characterInfo[SoupDescriptionText.textInfo.wordInfo[i].firstCharacterIndex + icon * SPACING_TEXT_FOR_ICON.Length];
                    var spaceLocation = SoupDescriptionText.transform.TransformPoint((firstSpacingChar.topLeft + firstSpacingChar.bottomLeft) / 2f);
                    FlavorIconTooltips[iconToolTipTracker].SetIcon(iconInfo, spaceLocation);
                }
                iconToolTipTracker++;
            }
            else display += word;
            display += ' ';
        }

        SoupDescriptionText.text = display;
        SoupDescriptionText.ForceMeshUpdate();
    }

    public void CloseBio()
    {
        BioHolder.gameObject.SetActive(false);
        currSoup = null;
    }

    public void OnCook(FinishedSoup newSoup)
    {
        if (currSoup == newSoup.soupBase)
        {
            ShowFinishedSoupBio(newSoup, true);
        }
    }
}