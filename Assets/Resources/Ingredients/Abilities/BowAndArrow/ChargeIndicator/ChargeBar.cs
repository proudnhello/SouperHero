using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeBar : ChargeIndicator
{
    public override void UpdateChargePercentage(float percentage)
    {
        // Clamp the percentage to be between 0 and 1
        percentage = Mathf.Clamp(percentage, 0, 1);
        print("Percentage: " + percentage + " resulting size: " + percentage*maxSize);
        transform.localScale = new Vector3(transform.localScale.x, percentage * maxSize, transform.localScale.z);
    }
}
