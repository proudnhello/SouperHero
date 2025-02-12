//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class InventoryManager : MonoBehaviour
//{

//    // make it a singleton
//    public static InventoryManager Singleton { get; private set; }

//    // make list of items
//    public List<Item> Items = new();

//    private void Awake()
//    {
//        if (Singleton != null && Singleton != this) Destroy(this);
//        else Singleton = this;
//    }

//    public void Add(Item item)
//    {
//        Items.Add(item);
//    }

//    public void Remove(Item item)
//    {
//        Items.Remove(item);
//    }
//}
