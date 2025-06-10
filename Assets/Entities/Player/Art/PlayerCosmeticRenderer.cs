using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCosmeticRenderer : MonoBehaviour
{
    public static PlayerCosmeticRenderer Singleton { get; private set; }

    public Material playerMaterial;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    public void SetPlayerCosmetic(Material material)
    {
        playerMaterial = material;
    }
}
