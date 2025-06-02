using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    int selectedEquippedSoup = 0;
    public void InitializeSlots(ISoupBowl[] bowls)
    {
        for (int i = 0; i < InventorySlots.Length; i++) InventorySlots[i].Init(i, bowls[i]);
        for (int i = 0; i < PlayerInventory.Singleton.maxEquippedSoups; i++)
        {
            if (i == selectedEquippedSoup) InventorySlots[i].EquipSlot();
            else InventorySlots[i].UnequipSlot();
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
        selectedSwapSlot = -1;
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
        foreach (var slot in InventorySlots) slot.EnterInventoryScreen();
    }
    public void CloseInventoryScreen()
    {
        if (CookingScreen.Singleton.IsCooking) return; // cannot close while cooking

        MoveInventory(false);
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlots[i].ExitInventoryScreen();
        }
        for (int i = 0; i < PlayerInventory.Singleton.maxEquippedSoups; i++)
        {
            if (i == selectedEquippedSoup) InventorySlots[i].EquipSlot();
            else InventorySlots[i].UnequipSlot();
        }
    }

    int selectedSwapSlot = -1;
    public void SetSelectedSoup(int slot)
    {
        if (selectedSwapSlot == -1)
        {
            selectedSwapSlot = slot;
            InventorySlots[slot].SelectSlot();
        }
        else if (slot == selectedSwapSlot)
        {
            selectedSwapSlot = -1;
            InventorySlots[slot].DeselectSlot();
        }
        else // Swap
        {
            InventorySlots[selectedSwapSlot].DeselectSlot();
            PlayerInventory.Singleton.SwapTwoSlots(slot, selectedSwapSlot);
            if (CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == slot) CookingScreen.Singleton.BowlCookingSlot.soupSlotReference = selectedSwapSlot;
            else if (CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == selectedSwapSlot) CookingScreen.Singleton.BowlCookingSlot.soupSlotReference = slot;
            InventorySlots[slot].SetSoup(PlayerInventory.Singleton.GetBowl(slot));
            InventorySlots[selectedSwapSlot].SetSoup(PlayerInventory.Singleton.GetBowl(selectedSwapSlot));
            selectedSwapSlot = -1;
        }
    }
    public int AddBowlToCookingSlot()
    {
        if (selectedSwapSlot < 0) return -1;
        if (InventorySlots[selectedSwapSlot].bowlHeld is not SoupBase) return -2;
        InventorySlots[selectedSwapSlot].AddBowlToCookingSlot();
        InventorySlots[selectedEquippedSoup].DeselectSlot();
        int slot = selectedSwapSlot;
        selectedSwapSlot = -1;
        return slot;
    }

    public void RemoveBowlCookingSlot(int slot)
    {
        InventorySlots[slot].RemoveBowlFromCookingSlot();
    }

    //Helper function to add soup image to icon in slot
    //Lo: This is temporary!
    //TODO: Once art is complete, use whatever image is attached to the soup instead of temp
    public void AddSoupInSlot(ISoupBowl bowl, int index)
    {
        InventorySlots[index].SetSoup(bowl);
    }

    public void ChangeUseCount()
    {
        InventorySlots[selectedEquippedSoup].UpdateUseCount();
    }
}
