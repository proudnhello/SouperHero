using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        public string sentence;
        public int stage;
    }
    public string nameKey;
    public DialogueLine[] sentenceKeys;
    internal int currentStage;
}
