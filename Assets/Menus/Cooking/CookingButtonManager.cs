using UnityEngine;
using UnityEngine.UI;

public class CookingButtonManager : MonoBehaviour
{
    public Button[] buttons; // Assign buttons in Inspector
    public Color baseColor = Color.white;
    public Color selectedColor = Color.green;
    public Button defaultButton;

    private void Start()
    {
        // Ensure all buttons start with the base color
        ResetButtonColors();

        // Set default button to selected color
        defaultButton.GetComponent<Image>().color = selectedColor;

        // Add listener to each button
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnButtonClick(btn));
        }
    }

    private void OnButtonClick(Button clickedButton)
    {
        // Reset all buttons to base color
        ResetButtonColors();

        // Change clicked button's color
        clickedButton.GetComponent<Image>().color = selectedColor;
    }

    private void ResetButtonColors()
    {
        foreach (Button btn in buttons)
        {
            btn.GetComponent<Image>().color = baseColor;
        }
    }
}
