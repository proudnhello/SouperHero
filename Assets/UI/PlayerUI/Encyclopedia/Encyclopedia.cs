using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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


    List<Ingredient> collectedEntries;
    void Awake()
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
        if (!collectedEntries.Contains(ing)) collectedEntries.Add(ing);
        Title.text = ing.IngredientName;
        EntryImage.sprite = ing.EncyclopediaImage;
        SourceText.text = ing.Source;
        if (ing.GetType() == typeof(FlavorIngredient))
        {
            FlavorEntry.text = ((FlavorIngredient)ing).FlavorProfile;
            FlavorEntry.gameObject.SetActive(true);
            AbilityEntry.gameObject.SetActive(false);
        } 
        else // is AbilityIngredient
        {
            AbilityEntry.text = ((AbilityIngredient)ing).AbilityDescription;
            AbilityEntry.gameObject.SetActive(true);
            FlavorEntry.gameObject.SetActive(false);
        }

        RenderedObject.SetActive(true);

    }
}
