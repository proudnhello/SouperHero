/*
 * An old version of this file was modified with the help of LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log10.md
 */

using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class SoupInventorySlot : MonoBehaviour, ICursorInteractable, ITooltipSource
{
    [SerializeField] TMP_Text usesText;
    [SerializeField] Image SlotContent;
    [SerializeField] Sprite EmptySoupSlotSprite;
    [SerializeField] float HoverTimeToDisplay;
    [SerializeField] float UnhoverTimeToHide;
    [SerializeField] Collider2D TooltipCollider;

    [Header("Anim")]
    [SerializeField] AnimationCurve ScaleCurve;
    [SerializeField] float ScaleAnimTime;
    [SerializeField] Vector3 EquippedBowlScale = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] Vector3 UnequippedBowlScale = new Vector3(.6f, .6f, .6f);
    [SerializeField] Color UnequippedSlotColor = new Color(.5f, .5f, .5f, .5f);
    [SerializeField] Vector3 UnhoveredBowlScale;
    [SerializeField] Vector3 HoveredBowlScaleOne;
    [SerializeField] Vector3 HoveredBowlScaleTwo;
    [SerializeField] AnimationCurve HoverScaleCurve;


    internal ISoupBowl bowlHeld;
    bool HasBowl { get => (bowlHeld is FinishedSoup || bowlHeld is SoupBase) && !isPickedUp && !isSelected; }
    int slotIndex;
    bool isSelected = false;
    bool isPickedUp = false;

    Vector3 DefaultPos;

    public void Init(int index, ISoupBowl bowl)
    {
        slotIndex = index;
        bowlHeld = bowl;
        DefaultPos = SlotContent.transform.localPosition;
        RenderSlotContents();
    }
    public void SetSoup(ISoupBowl bowl)
    {
        bowlHeld = bowl;
    }

    public void EquipSlot(bool immediately = false)
    {
        if (immediately)
        {
            SlotContent.color = Color.white;
            SlotContent.transform.localScale = EquippedBowlScale;
        }
        else
        {
            if (IEquipAnim != null) StopCoroutine(IEquipAnim);
            StartCoroutine(IEquipAnim = EquipAnim(true));
        }
        SoupInventoryUI.Singleton.EnableFlavorParticles(bowlHeld, this.gameObject);
        RenderSlotContents();
    }

    public void UnequipSlot(bool immediately = false)
    {
        if (immediately)
        {
            SlotContent.color = UnequippedSlotColor;
            SlotContent.transform.localScale = UnequippedBowlScale;
        }
        else
        {
            if (IEquipAnim != null) StopCoroutine(IEquipAnim);
            StartCoroutine(IEquipAnim = EquipAnim(false));
        }
        SoupInventoryUI.Singleton.DisableFlavorParticles(this.gameObject);
        RenderSlotContents();
    }

    public void SelectSlotForCooking()
    {
        isSelected = true;
        PlaceBowlInSlotAnim();
    }

    public void DeselectSlotForCooking()
    {
        isSelected = false;
        PlaceBowlInSlotAnim();
    }

    public void EnterInventoryScreen()
    {
        isSelected = CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == slotIndex;
        PlaceBowlInSlotAnim(true);
        SoupInventoryUI.Singleton.DisableFlavorParticles(this.gameObject);

    }

    public void ExitInventoryScreen()
    {
 
    }

    public void UpdateUseCount()
    {
        if (bowlHeld is FinishedSoup finishedSoup)
        {
            if (finishedSoup.uses < 0) usesText.text = "∞";
            else usesText.text = finishedSoup.uses.ToString();
        }
    }

    public void MouseDownOn() // select slot
    {
        if (HasBowl)
        {
            if (SoupInventoryUI.Singleton.IsOpen)
            {
                SoupInventoryUI.Singleton.ClickOnSlot(slotIndex);
                CursorManager.Singleton.PickupBowl(bowlHeld);
                SoupInventoryUI.Singleton.SoupBio.DragBowl(bowlHeld);
                PickUpBowlFromSlotAnim();
            }
        }
    }

    public void ReturnItemHereFromCursor()
    {
        SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
        if (bowlHeld != null)
        {
            if (!CursorManager.Singleton.TooltipTrigger.IsCursorHoveringOnTooltip(TooltipCollider))
            {
                SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(bowlHeld);
            }
            PlaceBowlInSlotAnim();
        }
    }


    public void MouseUpOn()
    {
        if (CursorManager.Singleton.currentBowlReference != null)
        {
            if (CursorManager.Singleton.currentBowlReference != bowlHeld)
            {
                if (SoupInventoryUI.Singleton.ReleaseOnSlot(slotIndex))
                {
                    CursorManager.Singleton.DropBowl();
                }
            }
            SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
        }
    }
    public void Tap()
    {
        if (CursorManager.Singleton.currentBowlReference != null && CursorManager.Singleton.currentBowlReference == bowlHeld) 
        {
            SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();    
            if (CookingScreen.Singleton.AtCookingScreen && bowlHeld is SoupBase) // add directly to available cooking slot
            {
                SoupInventoryUI.Singleton.TapSoupSlot(slotIndex);
                SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(bowlHeld);
                CursorManager.Singleton.DropBowl();
            }
        }
    }

    #region HOVERING
    public void OnHoverEnter()
    {
        if (HasBowl)
        {
            if (!SoupInventoryUI.Singleton.IsOpen && slotIndex >= PlayerInventory.Singleton.maxEquippedSoups) return;
            if (IHoverTimerForBio != null) StopCoroutine(IHoverTimerForBio);
            StartCoroutine(IHoverTimerForBio = HoverTimerForBio(true));
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            StartCoroutine(IHoverScaler = HoverScaler(true));
        }
    }

    IEnumerator IHoverTimerForBio;
    IEnumerator HoverTimerForBio(bool enter)
    {
        if (enter)
        {
            yield return new WaitForSeconds(HoverTimeToDisplay);
            SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(bowlHeld);
        }
        else
        {
            yield return new WaitForSeconds(UnhoverTimeToHide);
            SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(bowlHeld);
        }
    }


    public void OnHoverExit()
    {
        if (HasBowl)
        {
            if (!SoupInventoryUI.Singleton.IsOpen && slotIndex >= PlayerInventory.Singleton.maxEquippedSoups) return;
            if (IHoverTimerForBio != null) StopCoroutine(IHoverTimerForBio);
            StartCoroutine(IHoverTimerForBio = HoverTimerForBio(false));
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            StartCoroutine(IHoverScaler = HoverScaler(false));
        }
    }
    #endregion
    #region ANIMATION

    void PickUpBowlFromSlotAnim()
    {
        isPickedUp = true;
        if (IHoverScaler != null) StopCoroutine(IHoverScaler);
        if (IEquipAnim != null) StopCoroutine(IEquipAnim);
        RenderSlotContents();
    }

    void PlaceBowlInSlotAnim(bool enteringInventory = false) // either from swapping or returning bowl to slot or entering menu
    {
        hoverScaleAnimTime = 0;
        isPickedUp = false;
        RenderSlotContents();
        if (!HasBowl) return;

        if (enteringInventory) // equip every slot to normal scale
        {
            if (IEquipAnim != null) StopCoroutine(IEquipAnim);
            StartCoroutine(IEquipAnim = EquipAnim(true));
        }
        else
        {
            SlotContent.color = Color.white;
            SlotContent.transform.localScale = UnhoveredBowlScale;
        }

        //if mouse is already over, then trigger on hover anim
        if (CursorManager.Singleton.TooltipTrigger.IsCursorHoveringOnTooltip(TooltipCollider))
        {
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            StartCoroutine(IHoverScaler = HoverScaler(true));
        }
    }
    void RenderSlotContents()
    {
        SlotContent.enabled = true;
        SlotContent.rectTransform.sizeDelta = new Vector2(123, 75);
        usesText.text = "";
        if (bowlHeld is FinishedSoup finishedSoup && !isPickedUp && !isSelected)
        {
            SlotContent.sprite = finishedSoup.soupBase.finishedSprite;
            if (finishedSoup.uses < 0) usesText.text = "∞";
            else usesText.text = finishedSoup.uses.ToString();
        }
        else if (bowlHeld is SoupBase soupBase && !isPickedUp && !isSelected)
        {
            SlotContent.sprite = soupBase.baseSprite;
        }
        else
        {
            SlotContent.rectTransform.sizeDelta = new Vector2(82, 50);
            SlotContent.sprite = EmptySoupSlotSprite;
            SlotContent.enabled = SoupInventoryUI.Singleton.IsOpen;
            SlotContent.transform.localScale = Vector3.one;
            SlotContent.transform.localPosition = DefaultPos;
            SlotContent.color = Color.white;
        }
    }

    IEnumerator IHoverScaler;
    float hoverScaleAnimTime = 0;
    bool hasReachedMaxHover = false;
    IEnumerator HoverScaler(bool hover)
    {
        Vector3 baseScale = hover ? SlotContent.transform.localScale :
            !SoupInventoryUI.Singleton.IsOpen && slotIndex == SoupInventoryUI.Singleton.selectedEquippedSoup ? EquippedBowlScale :
            !SoupInventoryUI.Singleton.IsOpen && slotIndex != SoupInventoryUI.Singleton.selectedEquippedSoup ? UnequippedBowlScale :
            UnhoveredBowlScale;         // holy shit triple ternary operator im fucking coding

        Vector3 goalScale = hover ? HoveredBowlScaleOne : SlotContent.transform.localScale;

        if (hasReachedMaxHover && hoverScaleAnimTime > 0) hoverScaleAnimTime = ScaleAnimTime;
        hasReachedMaxHover = false;

        bool dir = hover;
        while (true)
        {
            while (hoverScaleAnimTime >= 0 && hoverScaleAnimTime <= ScaleAnimTime)
            {
                var percentCompleted = Mathf.Clamp01(hoverScaleAnimTime / ScaleAnimTime);
                var curveAmount = HoverScaleCurve.Evaluate(percentCompleted);
                SlotContent.transform.localScale = Vector3.Lerp(baseScale, goalScale, curveAmount);

                yield return null;
                hoverScaleAnimTime = dir ? hoverScaleAnimTime + Time.deltaTime : hoverScaleAnimTime - Time.deltaTime;
            }
            if (!hover) break; // only shrink once if unhovering
            // otherwise flip back and forth
            if (dir)
            {
                SlotContent.transform.localScale = HoveredBowlScaleOne;
                hoverScaleAnimTime = ScaleAnimTime;
            }
            else
            {
                SlotContent.transform.localScale = baseScale;
                hoverScaleAnimTime = 0;
            }

            dir = !dir;
            baseScale = HoveredBowlScaleTwo;
            hasReachedMaxHover = true;
        }

        SlotContent.transform.localScale = baseScale;
        hoverScaleAnimTime = 0;
        IHoverScaler = null;
    }

    IEnumerator IEquipAnim;
    IEnumerator EquipAnim(bool equip)
    {
        if (IHoverScaler != null) StopCoroutine(IHoverScaler);
        IHoverScaler = null;
        hoverScaleAnimTime = 0;

        Vector3 baseScale = equip ? SlotContent.transform.localScale : UnequippedBowlScale;
        Vector3 goalScale = !equip ? SlotContent.transform.localScale :
            !SoupInventoryUI.Singleton.IsOpen ? EquippedBowlScale :
            UnhoveredBowlScale;

        Color baseColor = equip ? SlotContent.color : UnequippedSlotColor;
        Color goalColor = !equip ? SlotContent.color : Color.white;

        float time = equip ? 0 : ScaleAnimTime;
        while (time >= 0 && time <= ScaleAnimTime)
        {
            var percentCompleted = Mathf.Clamp01(time / ScaleAnimTime);
            var curveAmount = ScaleCurve.Evaluate(percentCompleted);
            // only handle scaling if hover scaling isn't already doing it
            if (IHoverScaler == null) SlotContent.transform.localScale = Vector3.Lerp(baseScale, goalScale, curveAmount);
            SlotContent.color = Color.Lerp(baseColor, goalColor, curveAmount);

            yield return null;
            time = equip ? time + Time.deltaTime : time - Time.deltaTime;
        }

        if (equip)
        {
            if (IHoverScaler == null) SlotContent.transform.localScale = goalScale;
            SlotContent.color = goalColor;
        }
        else
        {
            if (IHoverScaler == null) SlotContent.transform.localScale = UnequippedBowlScale;
            SlotContent.color = UnequippedSlotColor;
        }
    }
    #endregion
}
