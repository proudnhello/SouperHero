using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;
using System;
public class Encyclopedia : MonoBehaviour
{
    [Serializable]
    public struct FlavorTextToIcon
    {
        public string KEY;
        public string REPLACEMENT_TEXT;
        public Sprite ICON;
    }
    public static Encyclopedia Singleton { get; private set; }

    [SerializeField] LayerMask ClickableLayers;
    [SerializeField] GameObject RenderedObject;
    [SerializeField] TMP_Text Title;
    [SerializeField] Image EntryImage;
    [SerializeField] TMP_Text SourceText;
    [SerializeField] TMP_Text FlavorEntry;
    [SerializeField] TMP_Text AbilityEntry;
    [SerializeField] Image[] FlavorIcons;
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

    public void PullUpEntry(Ingredient ing)
    {
        RenderedObject.SetActive(true);
        foreach (var icon in FlavorIcons) icon.gameObject.SetActive(false);

        if (!collectedEntries.Contains(ing)) collectedEntries.Add(ing);
        Title.text = ing.IngredientName;
        EntryImage.sprite = ing.EncyclopediaImage;
        SourceText.text = ing.Source;

        if (ing.GetType() == typeof(FlavorIngredient))
        {
            FlavorEntry.gameObject.SetActive(true);
            AbilityEntry.gameObject.SetActive(false);

            // PARSE FLAVORS IN TEXT AND REPLACE WITH ICONS
            //string pattern = @"(?<=(SOUR_Duration)|(SALTY_Crit)|(BITTER_Size)|(SWEET_Speed)|(SPICY_Burn)|(FROSTY_Freeze)|(HEARTY_Health)|(SPIKY_Damage)|(GREASY_Knockback)|(UMAMI_Vampirism))";
            string[] words = ((FlavorIngredient)ing).FlavorProfile.Split(' ');
            //string[] words = Regex.Matches(((FlavorIngredient)ing).FlavorProfile, pattern).Cast<Match>()
            //                    .Select(m => m.Value)
            //                    .ToArray();
            Debug.Log("word count = " + words.Length);
            string display = "";
            int icon = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                FlavorTextToIcon iconInfo;
                Debug.Log($"Word = {word}");
                if (FlavorTextToIconsDict.TryGetValue(word, out iconInfo))
                {

                    display += iconInfo.REPLACEMENT_TEXT;
                    FlavorEntry.text = display;
                    FlavorEntry.ForceMeshUpdate();
                    var firstCharInfo = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[i].firstCharacterIndex];
                    var wordLocation = FlavorEntry.transform.TransformPoint((firstCharInfo.topLeft + firstCharInfo.bottomLeft) / 2f);

                    FlavorIcons[icon].gameObject.SetActive(true);
                    FlavorIcons[icon].sprite = iconInfo.ICON;
                    FlavorIcons[icon].transform.position = wordLocation;
                    icon++;
                }
                else
                {
                    display += word;
                }
                display += ' ';
                Debug.Log("display is now = " + display);
            }

            FlavorEntry.text = display;

            //FlavorEntry.ForceMeshUpdate();
            //Debug.Log($"Word count = {FlavorEntry.textInfo.wordInfo.Length}");
            //spicyicon.SetActive(false);
            //foreach (var wordInfo in FlavorEntry.textInfo.wordInfo)
            //{
            //    Debug.Log($"Word = {wordInfo.firstCharacterIndex}");
            //}
            ////Debug.Log($"Word = {FlavorEntry.textInfo.wordInfo[1].firstCharacterIndex}");
            ////var firstCharInfo = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[2].firstCharacterIndex];
            ////var wordLocation = FlavorEntry.transform.TransformPoint((firstCharInfo.topLeft + firstCharInfo.bottomLeft) / 2f);
            ////Instantiate(spicyicon, wordLocation, transform.localRotation, transform);
            
        } 
        else // is AbilityIngredient
        {
            AbilityEntry.gameObject.SetActive(true);
            FlavorEntry.gameObject.SetActive(false);
            AbilityEntry.text = ((AbilityIngredient)ing).AbilityDescription;         
        }


    }
}
