using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

public class IngredientCookingSlot : MonoBehaviour, ICursorInteractable, ITooltipSource
{
    [SerializeField] Image faceImage;
    [SerializeField] Image slotOutline;
    [SerializeField] Image slotIcon;
    [SerializeField] Sprite[] slotIconSprites;
    internal Collectable ingredientReference;
    [SerializeField] RectTransform AbilityBio;
    [SerializeField] StatTooltip DamageStat;
    [SerializeField] StatTooltip DurationStat;
    [SerializeField] float ScaleAbilityBioAnimTime;
    [SerializeField] AnimationCurve ScaleAbilityBioAnimCurve;
    [SerializeField] Collider2D SlotCollider;

    public enum SlotType
    {
        Ability,
        Flavor,
        Wildcard
    }
    internal SlotType currentSlotType;

    public void Init()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
        faceImage.color = Color.white;
        slotOutline.gameObject.SetActive(true);
        AbilityBio.gameObject.SetActive(false);
        AbilityBio.localScale = new Vector3(0, 0, 0);
    }

    public void SetSlotType(SlotType type)
    {
        currentSlotType = type;
        slotIcon.sprite = slotIconSprites[(int)type];
        slotIcon.transform.localScale = new Vector3(.8f, .8f, .8f);
    }

    public void AddIngredient(Collectable ingredient)
    {
        ingredientReference = ingredient;
        ingredientReference.collectableUI.PlaceInCookingSlot(this);
        faceImage.gameObject.SetActive(true);
        faceImage.sprite = ingredientReference.collectableUI._SpriteReference;
        faceImage.color = Color.white;
        slotOutline.gameObject.SetActive(false);
        slotIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        CookingScreen.Singleton.CheckIfSoupIsValid();
    }

    public void MouseDownOn()
    {
        if (ingredientReference != null)
        {
            faceImage.color = new Color(1, 1, 1, .25f);
            CursorManager.Singleton.PickupCollectable(ingredientReference);
            IngredientBioDisplay.Singleton.DragIngredient(ingredientReference.ingredient);
        }
    }

    public void ReturnItemHereFromCursor()
    {
        faceImage.color = Color.white;
        if (ingredientReference == null) return;
        IngredientBioDisplay.Singleton.ReleaseDrag();
        if (!CursorManager.Singleton.TooltipTrigger.IsCursorHoveringOnTooltip(SlotCollider))
        {
            IngredientBioDisplay.Singleton.TryHideHoverBio(ingredientReference.ingredient);
        }
    }

    public void RemoveIngredient()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
        slotIcon.transform.localScale = new Vector3(.8f, .8f, .8f);
        slotOutline.gameObject.SetActive(true);
        CookingScreen.Singleton.CheckIfSoupIsValid();
    }

    public void Tap()
    {
        if (ingredientReference != null)
        {
            IngredientBioDisplay.Singleton.ReleaseDrag();
            IngredientBioDisplay.Singleton.TryHideHoverBio(ingredientReference.ingredient);
            ingredientReference.collectableUI.ReturnItemHereFromCursor();
            CursorManager.Singleton.DropCollectable();
        }
    }
    public void MouseUpOn()
    {
        if (CursorManager.Singleton.currentCollectableReference != null)
        {
            IngredientBioDisplay.Singleton.ReleaseDrag();
            if (currentSlotType == SlotType.Wildcard || (CursorManager.Singleton.currentCollectableReference.ingredient is AbilityIngredient && currentSlotType == SlotType.Ability) ||
                (CursorManager.Singleton.currentCollectableReference.ingredient is FlavorIngredient && currentSlotType == SlotType.Flavor))
            {
                if (ingredientReference != null) ingredientReference.collectableUI.ReturnItemHereFromCursor();
                AddIngredient(CursorManager.Singleton.currentCollectableReference);
                CursorManager.Singleton.DropCollectable();
            } else
            {
                CursorManager.Singleton.ManuallyReturnItemFromCursor();
            }
        }
    }

    public void OnCook()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
        slotIcon.transform.localScale = new Vector3(.8f, .8f, .8f);
        slotOutline.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public bool HasAbilityIngredient()
    {
        if (ingredientReference != null) if (ingredientReference.ingredient is AbilityIngredient) return true;
        return false;
    }

    public void EnterCookingScreen()
    {
        if (HasAbilityIngredient())
        {
            if (IScaleAnim != null) StopCoroutine(IScaleAnim);
            StartCoroutine(IScaleAnim = ScaleAbilityStats(true));
        }   
    }

    public void ExitCookingScreen()
    {
        HideAbilityStat();
    }
    public void SetAbilityStat(FinishedSoup.SoupAbility ability)
    {
        bool special = ability.IsInflictionSpecial(InflictionType.SPIKY_Damage);
        DamageStat.SetStat(ability.GetDamage(), special ? BioDatabase.Singleton.InflictionFlavorIcons[InflictionType.SPIKY_Damage].COLOR : Color.white);
        special = ability.IsBuffSpecial(BuffType.TOUGH_Duration);
        DurationStat.SetStat(ability.GetDuration(), special ? BioDatabase.Singleton.BuffFlavorIcons[BuffType.TOUGH_Duration].COLOR : Color.white);

        if (IScaleAnim != null) StopCoroutine(IScaleAnim);
        StartCoroutine(IScaleAnim = ScaleAbilityStats(true));
    }

    public void HideAbilityStat()
    {
        if (!AbilityBio.gameObject.activeInHierarchy) return;
        if (IScaleAnim != null) StopCoroutine(IScaleAnim);
        StartCoroutine(IScaleAnim = ScaleAbilityStats(false));
    }

    public void ResetAbilityStatBio()
    {
        AbilityBio.gameObject.SetActive(false);
        animTimeProgressed = 0;
        AbilityBio.localScale = Vector3.zero;
    }

    float animTimeProgressed = 0;
    IEnumerator IScaleAnim;
    IEnumerator ScaleAbilityStats(bool open)
    {
        if (open)
        {
            AbilityBio.gameObject.SetActive(true);
        }

        while (animTimeProgressed >= 0 && animTimeProgressed <= ScaleAbilityBioAnimTime)
        {
            var percentCompleted = Mathf.Clamp01(animTimeProgressed / ScaleAbilityBioAnimTime);
            var scaledPercentaged = ScaleAbilityBioAnimCurve.Evaluate(percentCompleted);
            var newScale = Mathf.Lerp(0, 1, scaledPercentaged);

            AbilityBio.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;

            animTimeProgressed = open ? animTimeProgressed + Time.deltaTime : animTimeProgressed - Time.deltaTime;
        }

        if (open)
        {
            animTimeProgressed = ScaleAbilityBioAnimTime;
            AbilityBio.localScale = Vector3.one;
        }
        else
        {
            ResetAbilityStatBio();
        }
    }

    #region HOVERING
    public void OnHoverEnter()
    {
        if (ingredientReference != null)
        {
            if (IHoverTimerForBio != null) StopCoroutine(IHoverTimerForBio);
            StartCoroutine(IHoverTimerForBio = HoverTimerForBio(true));
        }
    }

    IEnumerator IHoverTimerForBio;
    IEnumerator HoverTimerForBio(bool enter)
    {
        if (enter)
        {
            yield return new WaitForSeconds(IngredientBioDisplay.Singleton.HoverTimeToDisplay);
            IngredientBioDisplay.Singleton.TryDisplayHoverBio(ingredientReference.ingredient);
        }
        else
        {
            yield return new WaitForSeconds(IngredientBioDisplay.Singleton.HoverTimeToDisplay);
            IngredientBioDisplay.Singleton.TryHideHoverBio(ingredientReference.ingredient);
        }
    }


    public void OnHoverExit()
    {
        if (ingredientReference != null)
        {
            if (IHoverTimerForBio != null) StopCoroutine(IHoverTimerForBio);
            StartCoroutine(IHoverTimerForBio = HoverTimerForBio(false));
        }
    }
    #endregion
}
