using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownBar : MonoBehaviour
{
    public Image cooldownFill;

    void Update()
    {
        SoupSpoon currentSpoon = PlayerInventory.Singleton.GetSpoons()[PlayerInventory.Singleton.GetCurrentSpoon()];
        float fillNum = currentSpoon.GetCoolDownRatio();
        cooldownFill.fillAmount = fillNum;

        if (fillNum >= 1)
        {
            cooldownFill.enabled = false;
        }
        else
        {
            cooldownFill.enabled = true;

        }
    }
}
