using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] private GameObject soupPot;
    private static SpoonCounter spoonCounter;
    public int playerSpoons;

    [Header("Soup")]
    [SerializeField] private AbilityColorLookup colorLookup;
    [SerializeField] private AbilityLookup abilityLookup;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start() {
        spoonCounter = soupPot.GetComponent<SpoonCounter>();
    }

    void Update() {
        UpdateAbilities();
        // int newSpoonCount = PlayerManager.instance.GetAbilities().Count;
        // UpdateSpoons(newSpoonCount);
    }

    void UpdateSpoons(int count, List<AbilityAbstractClass> abilities) {
        if (playerSpoons < count) // Add spoons
        {
            for (int i = playerSpoons; i < count; i++) 
            {
                if (i < abilities.Count)
                {
                    // TODO: Implement color lookup for abilities
                    // Color color = abilityLookup.GetAbilityColor(abilities[i]);
                    Color color = Color.red;
                    spoonCounter.AddSpoon(color, playerSpoons);
                    playerSpoons++;
                }
            }
        }
        if (playerSpoons > count) {
            spoonCounter.DeleteSpoon(playerSpoons);
            playerSpoons--;
        }
    }

    public void UpdateAbilities() {
        int newSpoonCount = PlayerManager.instance.GetAbilities().Count;
        List<AbilityAbstractClass> abilities = PlayerManager.instance.GetAbilities();
        UpdateSpoons(newSpoonCount, abilities);
    }

}

