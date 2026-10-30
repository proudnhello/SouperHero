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
        }
        else
        {
            selector.gameObject.SetActive(false);
            cosmeticTitleText.text = "";
            unlockDescriptionText.text = "";
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
}
