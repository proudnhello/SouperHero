using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CosmeticSwitching : MonoBehaviour
{
    [SerializeField] SpriteRenderer player;
    [SerializeField] UnlockDatabase _database;
    private CosmeticData selectedCosmetic;
    private CosmeticData currCosmetic;
    [SerializeField] GameObject selectButton;
    [SerializeField] GameObject selectedText;
    [SerializeField] GameObject cosmeticText;
    [SerializeField] GameObject unlockText;
    [SerializeField] GameObject lockedText;
    [SerializeField] TextMeshProUGUI descriptionText;

    void Start()
    {
        FixCosmeticToSelected();
    }

    public void SwitchCosmetic(int direction)
    {
        int currIndex = _database.AllCosmetics.IndexOf(currCosmetic);

        int newIndex = currIndex + direction;
        if (newIndex < 0)
        {
            newIndex = _database.AllCosmetics.Count - 1;
        }

        currCosmetic = _database.AllCosmetics[newIndex % _database.AllCosmetics.Count];

        ChangeCosmeticText(currCosmetic.UUID);
        player.material = currCosmetic.Material;

        UpdateSelectUI();
    }

    private void ChangeCosmeticText(string cosmeticName)
    {
        cosmeticText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(cosmeticName);
        cosmeticText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(cosmeticName);
    }

    public void SetSelectedCosmetic()
    {
        selectedCosmetic = currCosmetic;
        PlayerCosmeticRenderer.Singleton.SetPlayerCosmetic(selectedCosmetic.Material);
        UnlockDataManager.Singleton.SetCosmetic(selectedCosmetic);
        UpdateSelectUI();
    }

    private void UpdateSelectUI()
    {
        if (selectedCosmetic == currCosmetic)  // Selected cosmetic already
        {
            selectedText.SetActive(true);
            selectButton.SetActive(false);
            unlockText.SetActive(false);
            lockedText.SetActive(false);
        }
        else
        {
            selectedText.SetActive(false);
            if (UnlockDataManager.Singleton.IsCosmeticUnlocked(currCosmetic.UUID))
            {   // Not selected cosmetic
                selectButton.SetActive(true);
                unlockText.SetActive(false);
                lockedText.SetActive(false);
            }
            else // Locked cosmetic
            {
                selectButton.SetActive(false);
                unlockText.SetActive(true);
                lockedText.SetActive(true);

                SetDescriptionText(currCosmetic);
            }
        }
    }

    public void SwapToSelectedCosmetic()
    {
        player.material = selectedCosmetic.Material;
    }

    public void FixCosmeticToSelected()
    {
        selectedCosmetic = currCosmetic = UnlockDataManager.Singleton.GetCurrentCosmetic();
        SwapToSelectedCosmetic();
        ChangeCosmeticText(currCosmetic.UUID);
        unlockText.SetActive(false);
        selectButton.SetActive(false);
        selectedText.SetActive(true);
        lockedText.SetActive(false);
    }

    private void SetDescriptionText(CosmeticData cosmetic)
    {
        foreach (var ach in UnlockDataManager.Singleton.database.AllAchievements)
        {
            if (ach.RewardedCosmetic == cosmetic)
            {
                descriptionText.SetText("Achieve \"" + ach.name + "\"");
            }
        }
    }
}
