using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string nameKey;
    [TextArea(2, 3)]
    public string[] sentenceKeys;
}
