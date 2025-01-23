using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Configuration")]

    public Texture2D cursorShootTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    public GameObject healthText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        Cursor.SetCursor(cursorShootTexture, hotSpot, cursorMode);
    }

    void HealthChange() {
        healthText.GetComponent<UnityEngine.UI.Text>().text = PlayerManager.instance.playerHealth.ToString();
    }

    void Update()
    {
        
    }
}
