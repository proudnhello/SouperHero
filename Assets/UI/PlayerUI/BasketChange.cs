using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BasketChange : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    CursorManager.Singleton.cursorObject.SetActive(false);
    //    CursorManager.Singleton.ShowCookingCursor();
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    if(!CursorManager.Singleton.isDragging)
    //    {
    //        CursorManager.Singleton.cursorObject.SetActive(true);
    //        CursorManager.Singleton.HideCookingCursor();
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("collided!");
        if (collision.gameObject.TryGetComponent<DraggableItem>(out DraggableItem dG))
        {
            dG.image.raycastTarget = true;
        }
    }
}
