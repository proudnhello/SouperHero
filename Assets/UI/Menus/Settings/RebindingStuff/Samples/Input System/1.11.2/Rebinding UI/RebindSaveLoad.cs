using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;
    public GameObject saveText;

    public void SaveSettings()
    {
        saveText.SetActive(true);
        OnDisable();
    }

    public void HideSaveText()
    {
        saveText.SetActive(false);
    }

    public void OnEnable()
    {
        if (saveText != null)
        {
            HideSaveText();
        }

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    public void OnDisable()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
}
