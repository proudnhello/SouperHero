using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : ScriptableObject
{
    public int id;
    public string ingredientName;
    public int value;
    public Sprite icon;
    [HideInInspector] public string ingredientType;

}
