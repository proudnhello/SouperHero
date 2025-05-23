using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeBar : ChargeIndicator
{
    SpriteRenderer spriteRenderer;
    public override void UpdateChargePercentage(float percentage)
    {
        // Clamp the percentage to be between 0 and 1
        percentage = Mathf.Clamp(percentage, 0, 1);
        transform.localScale = new Vector3(transform.localScale.x, percentage * maxSize, transform.localScale.z);
        // Debug.Log($"ChargeBar Scale: {transform.localScale}");
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (percentage == 1)
        {
            spriteRenderer.color = new Color(1, 0, 0, spriteRenderer.color.a);
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a);
        }
    }
}
