using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.VolumeComponent;

public class SoupInventoryUI : MonoBehaviour
{
    public static SoupInventoryUI Singleton { get; private set; }

    [Header("SoupInventory")]
    [SerializeField] RectTransform InventoryHolder;
    [SerializeField] SoupInventorySlot[] InventorySlots;

    [Header("Values")]
    [SerializeField] float ClosedYPos;
    [SerializeField] float OpenYPos;
    [SerializeField] AnimationCurve OpenAnimationCurve;
    [SerializeField] float OpenAnimationTime;

    [Header("Tooltip")]
    public SoupBioDisplay SoupBio;
    [SerializeField] GameObject SoupTooltip;
    [SerializeField] TMP_Text TooltipText;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
        InventoryHolder.localPosition = new Vector2(InventoryHolder.localPosition.x, ClosedYPos);
        CookingScreen.EnterCookingScreen += OpenInventoryScreen;
        CookingScreen.ExitCookingScreen += CloseInventoryScreen;
        PlayerInventory.ChangedEquippedSoup += ChangeEquippedSoup;
        PlayerInventory.UsedSoupAttack += ChangeUseCount;
    }
    private void Start()
    {
        SoupBio.Init(this);
    }

    internal int selectedEquippedSoup = 0;
    public void InitializeSlots(ISoupBowl[] bowls)
    {
        for (int i = 0; i < InventorySlots.Length; i++) InventorySlots[i].Init(i, bowls[i]);
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            if (i == selectedEquippedSoup) InventorySlots[i].EquipSlot(true);
            else InventorySlots[i].UnequipSlot(true);
        }
    }

    private void OnDisable()
    {
        CookingScreen.EnterCookingScreen -= OpenInventoryScreen;
        CookingScreen.ExitCookingScreen -= CloseInventoryScreen;
        PlayerInventory.ChangedEquippedSoup -= ChangeEquippedSoup;
        PlayerInventory.UsedSoupAttack -= ChangeUseCount;
    }

    public void ChangeEquippedSoup()
    {
        InventorySlots[selectedEquippedSoup].UnequipSlot();
        selectedEquippedSoup = PlayerInventory.Singleton.selectedEquippedSoup;
        InventorySlots[selectedEquippedSoup].EquipSlot();
    }

    public void ToggleInventory()
    {
        if (IsOpen) CloseInventoryScreen();
        else OpenInventoryScreen();
    }

    public void MoveInventory(bool open)
    {
        IsOpen = open;
        if (IMoveInventoryUI != null) StopCoroutine(IMoveInventoryUI);
        StartCoroutine(IMoveInventoryUI = MoveInventoryUI(open));
        heldSlot = -2;
    }

    float openAnimTimeProgressed;
    IEnumerator IMoveInventoryUI;
    internal bool IsOpen;
    private IEnumerator MoveInventoryUI(bool open)
    {

        while (openAnimTimeProgressed >= 0 && openAnimTimeProgressed <= OpenAnimationTime)
        {
            var percentCompleted = Mathf.Clamp01(openAnimTimeProgressed / OpenAnimationTime);
            var scaledPercentaged = OpenAnimationCurve.Evaluate(percentCompleted);
            var newYPos = Mathf.Lerp(ClosedYPos, OpenYPos, scaledPercentaged);

            InventoryHolder.localPosition = new Vector2(InventoryHolder.localPosition.x, newYPos);

            yield return null;

            openAnimTimeProgressed = open ? openAnimTimeProgressed + Time.deltaTime : openAnimTimeProgressed - Time.deltaTime;
        }

        openAnimTimeProgressed = open ? OpenAnimationTime : 0;
        InventoryHolder.localPosition = new Vector2(InventoryHolder.localPosition.x, open ? OpenYPos : ClosedYPos);
        IMoveInventoryUI = null;
    }

    public void OpenInventoryScreen()
    {
        MoveInventory(true);
        foreach (var slot in InventorySlots) { 
            DisableFlavorParticles(slot.gameObject);
            slot.EnterInventoryScreen(); 
        }
    }
    public void CloseInventoryScreen()
    {
        if (CookingScreen.Singleton.IsCooking) return; // cannot close while cooking

        MoveInventory(false);
        CursorManager.Singleton.ExitSoupInventory();
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            if (i == selectedEquippedSoup) InventorySlots[i].EquipSlot();
            else InventorySlots[i].UnequipSlot();
            InventorySlots[i].ExitInventoryScreen();
        }
    }

    int heldSlot = -2;
    public void ClickOnSlot(int slot) // cooking slot is -1
    {
        if (heldSlot == -2)
        {
            heldSlot = slot;
        }
    }
    public bool ReleaseOnSlot(int droppedOnSlot) // i could write this so much better but OOPS ALL EDGE CASES
    {
        if (heldSlot < -1) return false;

        void SwapSlots(int slot1, int slot2)
        {
            PlayerInventory.Singleton.SwapTwoSlots(slot1, slot2);
            InventorySlots[slot1].SetSoup(PlayerInventory.Singleton.GetBowl(slot1));
            InventorySlots[slot2].SetSoup(PlayerInventory.Singleton.GetBowl(slot2));
        }

        if (heldSlot == -1 && droppedOnSlot > -1) // drag from cooking bowl slot to inventory
        {
            if (droppedOnSlot == CookingScreen.Singleton.BowlCookingSlot.soupSlotReference)
            {
                InventorySlots[droppedOnSlot].DeselectSlotForCooking();
                CookingScreen.Singleton.DisplayNoBowl();
            }
            else if (InventorySlots[droppedOnSlot].bowlHeld is SoupBase)
            {
                SwapSlots(droppedOnSlot, CookingScreen.Singleton.BowlCookingSlot.soupSlotReference);
                InventorySlots[droppedOnSlot].DeselectSlotForCooking();
                CookingScreen.Singleton.DisplayBowlInSlot(CookingScreen.Singleton.BowlCookingSlot.soupSlotReference);
                InventorySlots[CookingScreen.Singleton.BowlCookingSlot.soupSlotReference].SelectSlotForCooking();
            }
            else if (InventorySlots[droppedOnSlot].bowlHeld is not FinishedSoup) // empty
            {
                SwapSlots(droppedOnSlot, CookingScreen.Singleton.BowlCookingSlot.soupSlotReference);
                InventorySlots[CookingScreen.Singleton.BowlCookingSlot.soupSlotReference].DeselectSlotForCooking();
                InventorySlots[droppedOnSlot].DeselectSlotForCooking();
                CookingScreen.Singleton.DisplayNoBowl();
            }
            else // drop on finished soup, return false so cursor knows to return bowl back to cooking slot
            {
                heldSlot = -2;
                return false;
            }
        }
        else if (droppedOnSlot == -1 && heldSlot > -1) // drag from inventory to cooking slot
        {
            if (InventorySlots[heldSlot].bowlHeld is SoupBase)
            {
                if (CookingScreen.Singleton.BowlCookingSlot.soupBaseReference != null)
                {
                    SwapSlots(heldSlot, CookingScreen.Singleton.BowlCookingSlot.soupSlotReference);
                    InventorySlots[heldSlot].DeselectSlotForCooking();
                    CookingScreen.Singleton.DisplayBowlInSlot(CookingScreen.Singleton.BowlCookingSlot.soupSlotReference);
                }
                else
                {
                    CookingScreen.Singleton.DisplayBowlInSlot(heldSlot);
                }
                InventorySlots[CookingScreen.Singleton.BowlCookingSlot.soupSlotReference].SelectSlotForCooking();
            }
        }
        else if (droppedOnSlot > -1 && heldSlot > -1) // between two slots in inventory
        {
            SwapSlots(droppedOnSlot, heldSlot);

            if (CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == droppedOnSlot)
            {
                CookingScreen.Singleton.BowlCookingSlot.soupSlotReference = heldSlot;
                InventorySlots[droppedOnSlot].DeselectSlotForCooking();
                InventorySlots[heldSlot].SelectSlotForCooking();
            }
            else if (CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == heldSlot)
            {
                CookingScreen.Singleton.BowlCookingSlot.soupSlotReference = droppedOnSlot;
                InventorySlots[heldSlot].DeselectSlotForCooking();
                InventorySlots[droppedOnSlot].SelectSlotForCooking();
            }
            else
            {
                InventorySlots[heldSlot].DeselectSlotForCooking();
                InventorySlots[droppedOnSlot].DeselectSlotForCooking();
            }
        }
        
        heldSlot = -2;
        return true;
    }

    public void TapSoupSlot(int slot)
    {
        heldSlot = slot;
        ReleaseOnSlot(-1);
        heldSlot = -2;
    }

    public void ReturnBowlFromCookingSlot(int slot)
    {
        InventorySlots[slot].DeselectSlotForCooking();
        CookingScreen.Singleton.DisplayNoBowl();
        heldSlot = -2;
    }

    public void DeselectSlot(int slot)
    {
        InventorySlots[slot].DeselectSlotForCooking();
        heldSlot = -2;
    }

    //Helper function to add soup image to icon in slot
    public void AddSoupInSlot(ISoupBowl bowl, int index)
    {
        InventorySlots[index].SetSoup(bowl);
    }

    public void ChangeUseCount()
    {
        InventorySlots[selectedEquippedSoup].UpdateUseCount();
    }

    public void OnCook(FinishedSoup newSoup, int index)
    {
        AddSoupInSlot(newSoup, index);
        DeselectSlot(index);
        SoupBio.OnCook(newSoup);
    }

    public void EnableFlavorParticles(ISoupBowl bowl, GameObject slot)
    {
        if (IsOpen || CookingScreen.Singleton.IsCooking) return; //Only display when inventory is closed and not cooking
        ParticleSystem particleSystem = slot.transform.GetComponentInChildren<ParticleSystem>(); //Access child particle game object on UI layer
        if (particleSystem == null) return;

        List<Sprite> particles = new List<Sprite> ();

        //Remove all particle icons from slot
        for (int i = 0; i < particleSystem.textureSheetAnimation.spriteCount; i++)
        {
            particleSystem.textureSheetAnimation.RemoveSprite(i);
        }

        //Add all ingredients' particle icon to list
        if (bowl is FinishedSoup soup)
        {
            foreach (var ingredient in soup.ingredientList)
            {
                if (ingredient is FlavorIngredient flav)
                {
                    foreach (var buff in flav.buffFlavors) particles.Add(BioDatabase.Singleton.BuffFlavorIcons[buff.buffType].ICON);
                    foreach (var inflict in flav.inflictionFlavors) particles.Add(BioDatabase.Singleton.InflictionFlavorIcons[inflict.inflictionType].ICON);
                }
                else if (ingredient is AbilityIngredient ab)
                {
                    foreach (var inflict in ab.inherentInflictionFlavors) particles.Add(BioDatabase.Singleton.InflictionFlavorIcons[inflict.inflictionType].ICON);
                }           
            }

            particles = particles.Distinct().ToList(); //Remove duplicates
        }

        //If no particles, do not enable particle effects
        if (particleSystem.textureSheetAnimation.mode == 0) return;

        //Add all particle icons and activate particle system for slot
        foreach (var particle in particles)
        {
            particleSystem.textureSheetAnimation.AddSprite(particle);
        }
        particleSystem.Play();
    }

    public void DisableFlavorParticles(GameObject slot)
    {
        ParticleSystem particleSystem = slot.transform.GetComponentInChildren<ParticleSystem>(); //Access child particle game object on UI layer
        if (particleSystem == null) return;

        particleSystem.Stop();
    }
}
