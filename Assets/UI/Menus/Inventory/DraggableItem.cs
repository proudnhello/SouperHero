using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public GameObject draggableItem;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public string ingredientType;

    [Header("Do Not Edit, Ingredient is Set In CookingUI's Enable()")]
    public Ingredient ingredient = null;
    public void OnBeginDrag(PointerEventData eventData)
    {
        // save the original parent
        parentAfterDrag = transform.parent;

        // bring the ingredient to the front of the scene while dragging
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // set raycast off so that when you drop on the slot
        // the drop system doesn't think you dropped it on itself
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // map item position to mouse position
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // set the parent to the parent after drag
        transform.SetParent(parentAfterDrag);

        // return raycast to true
        image.raycastTarget = true;
    }

}
