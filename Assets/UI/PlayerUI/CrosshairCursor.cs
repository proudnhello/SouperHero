using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairCursor : MonoBehaviour
{
    // [Header("Configuration")]

    private void Start()
    {
        CursorManager.Singleton.HideCursor();
    }

    private void OnEnable()
    {
        if (CursorManager.Singleton != null)
        {
            CursorManager.Singleton.HideCursor();
        }
    }

    private void OnDisable()
    {
        CursorManager.Singleton.ShowCursor();
    }

    public void Aim(bool isAiming) {
        Cursor.visible = isAiming;
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePos;
    }
}
