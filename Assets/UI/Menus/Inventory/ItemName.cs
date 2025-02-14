using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemName : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text.raycastTarget = false;
    }
}
