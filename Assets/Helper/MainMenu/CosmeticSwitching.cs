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

    void Awake()
    {
        foreach (CosmeticData cosmetic in _database.AllCosmetics)
        {
            if (cosmetic.Material = player.material)
            {
                selectedCosmetic = cosmetic;
                currCosmetic = cosmetic;
                break;
            }
        }

        ChangeCosmeticText(currCosmetic.UUID);
        unlockText.SetActive(false);
        selectButton.SetActive(false);
        selectedText.SetActive(true);
        lockedText.SetActive(false);
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

        if (CheckIfSelected())
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
            {
                selectButton.SetActive(true);
                unlockText.SetActive(false);
                lockedText.SetActive(false);
            }
            else
            {
                selectButton.SetActive(false);
                unlockText.SetActive(true);
                lockedText.SetActive(true);
            }
        }
    }

    private void ChangeCosmeticText(string cosmeticName)
    {
        cosmeticText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(cosmeticName);
        cosmeticText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(cosmeticName);
    }

    private bool CheckIfSelected()
    {
        if (selectedCosmetic == currCosmetic)
        {
            return (true);
        }
        return (false);
    }
}
