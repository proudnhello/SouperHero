using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : ScriptableObject
{
    [Header("Ingredient Info")]
    public string IngredientName;
    public Sprite Icon;
    public Sprite IconUI;
    public int uuid;

    [Header("Encyclopedia")]
    public Sprite EncyclopediaImage;
    public string Source;

}
