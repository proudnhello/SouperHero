using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickTester : MonoBehaviour
{

    public void OnSingleClick()
    {
        Debug.Log("Clicked one time!");
    }

    public void OnHoverEnter()
    {
        Debug.Log("Hovered in!");
    }

    public void OnHoverExit()
    {
        Debug.Log("Hovered out!");
    }
}
