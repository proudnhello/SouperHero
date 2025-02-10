using UnityEngine;

public class CursorController : MonoBehaviour
{
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; // Unlock if it was locked
    }

    public void HideCursor()
    {
        Cursor.visible = false;
    }
}
