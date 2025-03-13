using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BasketChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Singleton.cursorObject.SetActive(false);
        CursorManager.Singleton.ShowCookingCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!CursorManager.Singleton.isDragging)
        {
            CursorManager.Singleton.cursorObject.SetActive(true);
            CursorManager.Singleton.HideCookingCursor();
        }
    }
}
