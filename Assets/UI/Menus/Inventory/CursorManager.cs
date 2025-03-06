using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Singleton { get; private set; }
    public GameObject cursorObject;
    public CookingCursor cookingCursor;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; // Unlock if it was locked
        cursorObject.SetActive(false);
    }

    public void ShowCookingCursor()
    {
        cookingCursor.gameObject.SetActive(true);
    }

    public void HideCookingCursor()
    {
        cookingCursor.gameObject.SetActive(false);
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        cursorObject.SetActive(true);
    }
}
