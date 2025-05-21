using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class ChargeIndicator : MonoBehaviour
{
    protected float maxSize = 0.5f;
    public abstract void UpdateChargePercentage(float percentage);
}
