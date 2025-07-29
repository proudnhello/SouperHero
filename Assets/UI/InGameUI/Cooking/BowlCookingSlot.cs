using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BowlCookingSlot : MonoBehaviour, ICursorInteractable, ITooltipSource
{
    [SerializeField] Image SlotOutline;
    [SerializeField] Image SlotContent;
    [SerializeField] Image EmptySlotIcon;
    [SerializeField] float HoverTimeToDisplay;
    [SerializeField] float UnhoverTimeToHide;
    [SerializeField] Collider2D TooltipCollider;

    [Header("Anim")]
    [SerializeField] float ScaleAnimTime;
    [SerializeField] Vector3 UnhoveredBowlScale;
    [SerializeField] Vector3 HoveredBowlScaleOne;
    [SerializeField] Vector3 HoveredBowlScaleTwo;
    [SerializeField] AnimationCurve HoverScaleCurve;

    internal int soupSlotReference = -1;
    internal SoupBase soupBaseReference = null;
    public void EnterCookingScreen()
    {
        if (soupBaseReference != null) SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(soupBaseReference);

    }
    public void ExitCookingScreen()
    {
        if (soupBaseReference != null)
        {
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            StartCoroutine(IHoverScaler = HoverScaler(false));
        }
    }

    public void MouseDownOn()
    {
        if (soupBaseReference != null)
        {
            if (!CookingScreen.Singleton.IsCooking) return;

            SoupInventoryUI.Singleton.ClickOnSlot(-1);
            CursorManager.Singleton.PickupBowl(soupBaseReference);
            RenderSlot(false);
        }
    }

    public void MouseUpOn()
    {
        if (CursorManager.Singleton.currentBowlReference != null &&
            CursorManager.Singleton.currentBowlReference != (ISoupBowl)soupBaseReference)
        {
            SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
            SoupInventoryUI.Singleton.ReleaseOnSlot(-1);
            CursorManager.Singleton.DropBowl();
        }
    }

    public void Tap()
    {
        if (CursorManager.Singleton.currentBowlReference != null &&
            CursorManager.Singleton.currentBowlReference == (ISoupBowl)soupBaseReference)
        {
            SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(soupBaseReference);
            SoupInventoryUI.Singleton.ReturnBowlFromCookingSlot(soupSlotReference);
            CursorManager.Singleton.DropBowl();
        }
    }

    public void ReturnItemHereFromCursor()
    {
        SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
        RenderSlot(soupBaseReference != null);
    }

    public void RenderSlot(bool displayBowl)
    {
        hoverScaleAnimTime = 0;

        if (soupBaseReference != null && displayBowl)
        {
            SlotOutline.gameObject.SetActive(false);
            SlotContent.gameObject.SetActive(true);
            SlotContent.sprite = soupBaseReference.baseSprite;
            EmptySlotIcon.gameObject.SetActive(false);
            SlotContent.transform.localScale = UnhoveredBowlScale;
            if (CursorManager.Singleton.TooltipTrigger.IsCursorHoveringOnTooltip(TooltipCollider))
            {
                SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(soupBaseReference);
                if (IHoverScaler != null) StopCoroutine(IHoverScaler);
                StartCoroutine(IHoverScaler = HoverScaler(true));
            }
        }
        else
        {
            SlotOutline.gameObject.SetActive(true);
            EmptySlotIcon.gameObject.SetActive(true);
            SlotContent.gameObject.SetActive(false);
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            hoverScaleAnimTime = 0;
        }
    }

    public void AddBowlFromSlot(int slot)
    {
        soupSlotReference = slot;
        soupBaseReference = (SoupBase)PlayerInventory.Singleton.GetBowl(slot);
        RenderSlot(true);
    }

    public void RemoveBowl()
    {
        soupBaseReference = null;
        soupSlotReference = -1;
        RenderSlot(false);
    }

    public void OnHoverEnter()
    {
        if (soupBaseReference != null)
        {
            if (IHoverTimer != null) StopCoroutine(IHoverTimer);
            StartCoroutine(IHoverTimer = HoverTimer(true));
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            StartCoroutine(IHoverScaler = HoverScaler(true));
        }
    }

    IEnumerator IHoverTimer;
    IEnumerator HoverTimer(bool enter)
    {
        if (enter)
        {
            yield return new WaitForSeconds(HoverTimeToDisplay);
            if (soupBaseReference != null) SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(soupBaseReference);
        }
        else
        {
            yield return new WaitForSeconds(UnhoverTimeToHide);
            if (soupBaseReference != null) SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(soupBaseReference);
        }
    }

    public void OnHoverExit()
    {
        if (soupBaseReference != null)
        {
            if (IHoverTimer != null) StopCoroutine(IHoverTimer);
            StartCoroutine(IHoverTimer = HoverTimer(false));
            if (IHoverScaler != null) StopCoroutine(IHoverScaler);
            StartCoroutine(IHoverScaler = HoverScaler(false));
        }
    }

    IEnumerator IHoverScaler;
    float hoverScaleAnimTime = 0;
    bool hasReachedMaxHover = false;
    IEnumerator HoverScaler(bool hover)
    {
        Vector3 baseScale = hover ? SlotContent.transform.localScale : UnhoveredBowlScale;  

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
}
