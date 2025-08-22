using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMatchSlider : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text text;
    [SerializeField] private UnityEngine.UI.Slider slider;

    private void Start()
    {
        UpdateText();
        slider.onValueChanged.AddListener(delegate { UpdateText(); });
    }

    private void UpdateText()
    {
        text.text = (int)(slider.value * 100) + "%";
    }
}
