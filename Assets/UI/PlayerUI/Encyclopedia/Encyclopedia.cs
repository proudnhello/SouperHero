using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml;
public class Encyclopedia : MonoBehaviour
{
    public static Encyclopedia Singleton { get; private set; }

    [SerializeField] LayerMask ClickableLayers;
    [SerializeField] GameObject RenderedObject;
    [SerializeField] TMP_Text Title;
    [SerializeField] Image EntryImage;
    [SerializeField] TMP_Text SourceText;
    [SerializeField] TMP_Text FlavorEntry;
    [SerializeField] TMP_Text AbilityEntry;
    [SerializeField] GameObject spicyicon;

    Dictionary<BuffType, string> buffFlavorToIconSpacing = new()
    {
        
    };
    Dictionary<InflictionType, string> inflictionFlavorToIconSpacing = new()
    {
        { InflictionType.SPICY_Burn, "                " }
    };


    List<Ingredient> collectedEntries;
    private void Awake()
    {
        RenderedObject.SetActive(false);
    }
    void Start()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;

        collectedEntries = new();
        RenderedObject.SetActive(false);

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

        if (!collectedEntries.Contains(ing)) collectedEntries.Add(ing);
        Title.text = ing.IngredientName;
        EntryImage.sprite = ing.EncyclopediaImage;
        SourceText.text = ing.Source;

        if (ing.GetType() == typeof(FlavorIngredient))
        {
            // PARSE FLAVORS IN TEXT AND REPLACE WITH ICONS
            //string pattern = "\b(SOUR_Duration|SALTY_Crit|BITTER_Size|SWEET_Speed|SPICY_Burn|FROSTY_Freeze|HEARTY_Health|SPIKY_Damage|GREASY_Knockback|UMAMI_Vampirism|)\b";
            //SourceText.textInfo.characterInfo
            //string[] words = Regex.Matches(((FlavorIngredient)ing).FlavorProfile, pattern).Cast<Match>()
            //                    .Select(m => m.Value)
            //                    .ToArray();

            //string display = "";
            //foreach (var word in words)
            //{

            //}
            FlavorEntry.gameObject.SetActive(true);
            AbilityEntry.gameObject.SetActive(false);

            FlavorEntry.text = ((FlavorIngredient)ing).FlavorProfile;

            FlavorEntry.ForceMeshUpdate();
            Debug.Log($"Word count = {FlavorEntry.textInfo.wordInfo.Length}");
            foreach (var wordInfo in FlavorEntry.textInfo.wordInfo)
            {
                Debug.Log($"Word = {wordInfo.firstCharacterIndex}");
            }
            //Debug.Log($"Word = {FlavorEntry.textInfo.wordInfo[1].firstCharacterIndex}");
            var firstCharInfo = FlavorEntry.textInfo.characterInfo[FlavorEntry.textInfo.wordInfo[2].firstCharacterIndex];
            var wordLocation = FlavorEntry.transform.TransformPoint((firstCharInfo.topLeft + firstCharInfo.bottomLeft) / 2f);
            Instantiate(spicyicon, wordLocation, transform.localRotation, transform);
            
        } 
        else // is AbilityIngredient
        {
            AbilityEntry.gameObject.SetActive(true);
            FlavorEntry.gameObject.SetActive(false);

            AbilityEntry.text = ((AbilityIngredient)ing).AbilityDescription;
            AbilityEntry.ForceMeshUpdate();
            Debug.Log($"Word count = {AbilityEntry.textInfo.wordInfo.Length}");
            foreach (var wordInfo in AbilityEntry.textInfo.wordInfo)
            {
                Debug.Log($"Word = {wordInfo.firstCharacterIndex}");
            }
            //Debug.Log($"Word = {FlavorEntry.textInfo.wordInfo[1].firstCharacterIndex}");
            var firstCharInfo = AbilityEntry.textInfo.characterInfo[AbilityEntry.textInfo.wordInfo[2].firstCharacterIndex];
            var wordLocation = AbilityEntry.transform.TransformPoint((firstCharInfo.topLeft + firstCharInfo.bottomLeft) / 2f);
            Instantiate(spicyicon, wordLocation, transform.localRotation, transform);
            
        }


    }
}
