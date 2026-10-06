using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoupCollectable : Collectable
{
    [SerializeField] SoupBase soup;
    public override void Collect()
    {
        PlayerInventory.Singleton.AddBowlToInventory(soup);
        Destroy(gameObject);
    }
}
