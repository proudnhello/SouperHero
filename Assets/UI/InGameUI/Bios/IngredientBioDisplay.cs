using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

using System;
using Unity.VisualScripting;
public class IngredientBioDisplay : MonoBehaviour
{

    public static IngredientBioDisplay Singleton { get; private set; }

    [Header("Links")]
    [SerializeField] GameObject ParentObject;
    [SerializeField] TMP_Text Title;
    [SerializeField] TMP_Text IngredientTypeHeader;
    [SerializeField] GameObject FlavorSection;
    [SerializeField] TMP_Text FlavorEntry;
    [SerializeField] BasketFlavorIconTooltip[] FlavorIconTooltips;
    [SerializeField] GameObject AbilitySection;
    [SerializeField] TMP_Text AbilityEntry;
    [SerializeField] Image AbilityIcon;
    [SerializeField] StatTooltip DamageStat;
    [SerializeField] StatTooltip DurationStat;

    [Header("Values")]
    [SerializeField] string SPACING_TEXT_FOR_ICON;
    [SerializeField] Color FlavorHeaderColor;
    [SerializeField] Color AbilityHeaderColor;


    void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;

        ParentObject.SetActive(false);
    }

    private void Update()
    {
        if (ParentObject.activeInHierarchy)
        {
            if (PlayerEntityManager.Singleton.playerMovement.IsMoving() && !CursorManager.Singleton.IsHoldingSomething)
            {
                ParentObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        ParentObject.SetActive(false);
    }

    public void PullUpBio(Ingredient ing)
    {
        /*
        Note: Right now the localization table is storing the key to each string as whatever text is stored in the ingredient csv file.
        This is fine for now, but if we want to change the key to be something more readable, we should do that, but we'll need
        to change how the ingredient spreadsheets are organized so that they use keys instead of the raw text.
        - Igor 
        */

        ParentObject.SetActive(true);
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
            DamageStat.SetStat(dmg);

            DurationStat.SetStat(((AbilityIngredient)ing).baseStats.BaseDuration);
        }
    }
}
