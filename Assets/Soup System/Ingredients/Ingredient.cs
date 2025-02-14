using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : ScriptableObject
{
    [Header("Ingredient Info")]
    public int id;
    public string ingredientName;
    public Sprite icon;
}
