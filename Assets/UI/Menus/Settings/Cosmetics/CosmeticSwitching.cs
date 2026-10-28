using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.UI;

public class CosmeticSwitching : MonoBehaviour
{
    [SerializeField] SpriteRenderer menuPlayer;
    [SerializeField] UnlockDatabase _database;
    private CosmeticData selectedCosmetic;
    private CosmeticButton selectedCosmeticButton;

    [SerializeField] CosmeticButton[] cosmeticButtons;
    [SerializeField] RectTransform selector;
    [SerializeField] TMP_Text cosmeticTitleText;
    [SerializeField] TMP_Text unlockDescriptionText;

    void Start()
    {
        selectedCosmetic = UnlockDataManager.Singleton.GetCurrentCosmetic();
        menuPlayer.material = selectedCosmetic.Material;
        foreach (CosmeticButton button in cosmeticButtons)
        {
            button.Init();

            if (!UnlockDataManager.Singleton.IsCosmeticUnlocked(button.cosmeticUnlock.UUID))
            {
                button.SetAsLocked();
            }

            // Connecting event trigger to hovered event
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, () => OnButtonHover(button, true));
            AddEvent(trigger, EventTriggerType.PointerExit, () => OnButtonHover(button, false));

            if (button.cosmeticUnlock == selectedCosmetic)
            {
                selectedCosmeticButton = button;
                button.SelectCosmetic();
            }
        }

    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }
    private void OnButtonHover(CosmeticButton _button, bool hoverOn)
    {
        if (hoverOn)
        {
            selector.gameObject.SetActive(true);
            selector.localPosition = _button.transform.localPosition;
            cosmeticTitleText.text = LocalizationManager.GetLocalizedString(_button.cosmeticUnlock.UUID);
            if (_button.achievementUnlock != null) unlockDescriptionText.text = LocalizationManager.GetLocalizedString("Achieve") + LocalizationManager.GetLocalizedString(_button.achievementUnlock.name) + "\"";
            else unlockDescriptionText.text = "";
            //_button.OnHover(_animTime, _scaleFactor);
        }
        else
        {
            selector.gameObject.SetActive(false);
            //_button.OnUnhover(_animTime);
        }
    }

    public void TrySelectCosmetic(int index)
    {
        if (UnlockDataManager.Singleton.IsCosmeticUnlocked(cosmeticButtons[index].cosmeticUnlock.UUID))
        {   // Not selected cosmetic
            selectedCosmeticButton.DeselectButton();
            selectedCosmeticButton = cosmeticButtons[index];
            selectedCosmetic = selectedCosmeticButton.cosmeticUnlock;
            menuPlayer.material = selectedCosmetic.Material;
            PlayerCosmeticRenderer.Singleton.SetPlayerCosmetic(selectedCosmetic.Material);
            UnlockDataManager.Singleton.SetCosmetic(selectedCosmetic);
            cosmeticButtons[index].SelectCosmetic();
        }
    }

    //public void SwitchCosmetic(int direction)
    //{
    //    int currIndex = _database.AllCosmetics.IndexOf(currCosmetic);

    //    int newIndex = currIndex + direction;
    //    if (newIndex < 0)
    //    {
    //        newIndex = _database.AllCosmetics.Count - 1;
    //    }

    //    currCosmetic = _database.AllCosmetics[newIndex % _database.AllCosmetics.Count];

    //    ChangeCosmeticText(currCosmetic.UUID);

    //    UpdateSelectUI();
    //}

    //private void ChangeCosmeticText(string cosmeticName)
    //{
    //    string localizedCosmeticName = LocalizationManager.GetLocalizedString(cosmeticName);
    //    cosmeticText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(localizedCosmeticName);
    //    cosmeticText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(localizedCosmeticName);
    //}

    //public void SetSelectedCosmetic()
    //{
    //    selectedCosmetic = currCosmetic;
    //    PlayerCosmeticRenderer.Singleton.SetPlayerCosmetic(selectedCosmetic.Material);
    //    UnlockDataManager.Singleton.SetCosmetic(selectedCosmetic);
    //    UpdateSelectUI();
    //}

    //private void UpdateSelectUI()
    //{
    //    if (selectedCosmetic == currCosmetic)  // Selected cosmetic already
    //    {
    //        selectedText.SetActive(true);
    //        selectButton.SetActive(false);
    //        unlockText.SetActive(false);
    //        lockedText.SetActive(false);
    //    }
    //    else
    //    {
    //        selectedText.SetActive(false);
    //        if (UnlockDataManager.Singleton.IsCosmeticUnlocked(currCosmetic.UUID))
    //        {   // Not selected cosmetic
    //            selectButton.SetActive(true);
    //            unlockText.SetActive(false);
    //            lockedText.SetActive(false);
    //        }
    //        else // Locked cosmetic
    //        {
    //            selectButton.SetActive(false);
    //            unlockText.SetActive(true);
    //            lockedText.SetActive(true);

    //            SetDescriptionText(currCosmetic);
    //        }
    //    }
    //}

    //public void SwapToSelectedCosmetic()
    //{
    //    player.material = selectedCosmetic.Material;
    //}

    //public void FixCosmeticToSelected()
    //{
    //    selectedCosmetic = currCosmetic = UnlockDataManager.Singleton.GetCurrentCosmetic();
    //    SwapToSelectedCosmetic();
    //    ChangeCosmeticText(currCosmetic.UUID);
    //    unlockText.SetActive(false);
    //    selectButton.SetActive(false);
    //    selectedText.SetActive(true);
    //    lockedText.SetActive(false);
    //}

    //private void SetDescriptionText(CosmeticData cosmetic)
    //{
    //    foreach (var ach in UnlockDataManager.Singleton.database.AllAchievements)
    //    {
    //        if (ach.RewardedCosmetic == cosmetic)
    //        {
    //            string localizedText = LocalizationManager.GetLocalizedString("Achieve") + LocalizationManager.GetLocalizedString(ach.name) + "\"";
    //            //descriptionText.SetText("Achieve \"" + ach.name + "\"");
    //            descriptionText.SetText(localizedText);
    //        }
    //    }
    //}
}
