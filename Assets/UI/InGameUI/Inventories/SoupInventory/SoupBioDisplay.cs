using System;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SoupBioDisplay : MonoBehaviour
{
    [SerializeField] RectTransform BioHolder;
    [SerializeField] TMP_Text TitleText;
    [SerializeField] GameObject SoupBaseSection;
    [SerializeField] FlavorIconTextTooltip[] FlavorIconTooltips;
    [SerializeField] TMP_Text BaseDescriptionText;


    [SerializeField] GameObject FinishedSoupSection;
    [SerializeField] string SPACING_TEXT_FOR_ICON;

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
            Debug.Log(bowl == currLockedSoup);
            if (bowl == currLockedSoup)
            {
                UnlockSlot();
            }
            else // Lock slot
            {
                currLockedSoup = bowl;
                Debug.Log("locking in " + bowl.baseName);
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

        #region FlavorIconText
        foreach (var icon in FlavorIconTooltips) icon.ClearIcons();

        // PARSE FLAVORS IN TEXT AND REPLACE WITH ICONS
        string localizedstr = LocalizationManager.GetLocalizedString(soup.baseName + " Profile");  
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
                BaseDescriptionText.text = display;
                BaseDescriptionText.ForceMeshUpdate();

                var p1Char = BaseDescriptionText.textInfo.characterInfo[BaseDescriptionText.textInfo.wordInfo[i].firstCharacterIndex];
                var p2Char = BaseDescriptionText.textInfo.characterInfo[BaseDescriptionText.textInfo.wordInfo[i].lastCharacterIndex];
                FlavorIconTooltips[iconToolTipTracker].SetBounds(
                    BaseDescriptionText.transform.TransformPoint(p1Char.bottomLeft),
                    BaseDescriptionText.transform.TransformPoint(p2Char.topRight)
                );

                for (int icon = 0; icon < iconCount; icon++)
                {
                    var firstSpacingChar = BaseDescriptionText.textInfo.characterInfo[BaseDescriptionText.textInfo.wordInfo[i].firstCharacterIndex + icon * SPACING_TEXT_FOR_ICON.Length];
                    var spaceLocation = BaseDescriptionText.transform.TransformPoint((firstSpacingChar.topLeft + firstSpacingChar.bottomLeft) / 2f);
                    FlavorIconTooltips[iconToolTipTracker].SetIcon(iconInfo, spaceLocation);
                }
                iconToolTipTracker++;
            }
            else display += word;
            display += ' ';
        }

        BaseDescriptionText.text = display;
        BaseDescriptionText.ForceMeshUpdate();
        #endregion

    }

    void ShowFinishedSoupBio(FinishedSoup soup)
    {
        if (currSoup == soup.soupBase) return;
        currSoup = soup.soupBase;

        BioHolder.gameObject.SetActive(true);
        FinishedSoupSection.SetActive(true);
        SoupBaseSection.SetActive(false);
        TitleText.text = LocalizationManager.GetLocalizedString(soup.soupBase.baseName);
    }

    public void CloseBio()
    {
        BioHolder.gameObject.SetActive(false);
        currSoup = null;
    }
}