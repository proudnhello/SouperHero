using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SoupAbilitiesUI : MonoBehaviour
{

    public List<Image> children;
    public List<TextMeshProUGUI> usesText;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            children.Add(child.GetComponent<Image>());
            usesText.Add(child.GetChild(0).GetComponent<TextMeshProUGUI>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
