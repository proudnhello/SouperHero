using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairCursor : MonoBehaviour
{
    // [Header("Configuration")]

    void Awake() {
        CursorManager.Singleton.HideCursor();
    }

    private void OnEnable()
    {
        CursorManager.Singleton.HideCursor();
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
