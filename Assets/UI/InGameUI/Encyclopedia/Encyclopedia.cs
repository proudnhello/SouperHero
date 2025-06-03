using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;
using System;
using JetBrains.Annotations;
public class Encyclopedia : MonoBehaviour
{
    [Serializable]
    public struct FlavorTextToIcon
    {
        public string KEY;
        public string REPLACEMENT_TEXT;
        public Sprite ICON;
        public string TOOLTIP_TEXT;
    }
    public static Encyclopedia Singleton { get; private set; }

    [SerializeField] LayerMask ClickableLayers;
    [SerializeField] GameObject RenderedObject;
    [SerializeField] TMP_Text Title;
    [SerializeField] Image EntryImage;
    [SerializeField] TMP_Text SourceText;
    [SerializeField] GameObject FlavorProfile;
    [SerializeField] TMP_Text FlavorEntry;
    [SerializeField] TMP_Text AbilityEntry;
    [SerializeField] EncyclopediaFlavorIcon[] FlavorIcons;
    [SerializeField] List<FlavorTextToIcon> FlavorTextToIcons;

    Dictionary<string, FlavorTextToIcon> FlavorTextToIconsDict = new();


    List<Ingredient> collectedEntries;

    void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;

        collectedEntries = new();
        RenderedObject.SetActive(false);
        foreach (var flavor in FlavorTextToIcons)
        {
            FlavorTextToIconsDict.Add(flavor.KEY, flavor);
        }

    }

    private void Update()
    {
        if (RenderedObject.activeInHierarchy)
        {
            if (PlayerEntityManager.Singleton.playerMovement.IsMoving())
            {
                RenderedObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        RenderedObject.SetActive(false);
    }

    public void PullUpEntry(Ingredient ing)
    {
        /*
        Note: Right now the localization table is storing the key to each string as whatever text is stored in the ingredient csv file.
        This is fine for now, but if we want to change the key to be something more readable, we should do that, but we'll need
        to change how the ingredient spreadsheets are organized so that they use keys instead of the raw text.
        - Igor 
        */

        RenderedObject.SetActive(true);
        foreach (var icon in FlavorIcons) icon.gameObject.SetActive(false);

        if (!collectedEntries.Contains(ing)) collectedEntries.Add(ing);
        Title.text = LocalizationManager.GetLocalizedString(ing.IngredientName);    // Localize ingredient name
        EntryImage.sprite = ing.EncyclopediaImage;
        SourceText.text = LocalizationManager.GetLocalizedString(ing.Source);    // Localize ingredient source

        if (ing.GetType() == typeof(FlavorIngredient))
        {
            FlavorProfile.SetActive(true);
            AbilityEntry.gameObject.SetActive(false);

            // PARSE FLAVORS IN TEXT AND REPLACE WITH ICONS
            string localizedstr = LocalizationManager.GetLocalizedString(((FlavorIngredient)ing).FlavorProfile);    // Localize flavor profile
            string[] words = localizedstr.Split(' ');
            string display = "";
            int icon = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                FlavorTextToIcon iconInfo;
                if (FlavorTextToIconsDict.TryGetValue(word, out iconInfo))
                {

                    display += iconInfo.REPLACEMENT_TEXT;
                    FlavorEntry.text = display;
                    FlavorEntry.ForceMeshUpdate();
                    var firstCharInfo = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[i].firstCharacterIndex];
                    var wordLocation = FlavorEntry.transform.TransformPoint((firstCharInfo.topLeft + firstCharInfo.bottomLeft) / 2f);

                    FlavorIcons[icon].SetIcon(iconInfo);
                    FlavorIcons[icon].transform.position = wordLocation;
                    icon++;
                }
                else
                {
                    display += word;
                }
                display += ' ';
            }

            FlavorEntry.text = display;
            
        } 
        else // is AbilityIngredient
        {
            AbilityEntry.gameObject.SetActive(true);
            FlavorProfile.SetActive(false);
            AbilityEntry.text = LocalizationManager.GetLocalizedString(((AbilityIngredient)ing).AbilityDescription);   // Localize ability description
        }
    }
}
