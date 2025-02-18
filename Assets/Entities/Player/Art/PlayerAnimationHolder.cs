using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Animations/Player Animations")]
public class PlayerAnimationHolder : ScriptableObject
{
    [Header("Animations")]
    public Sprite[] IdleSprites;
    public int IdleFPS;
    public Sprite[] WalkSprites;
    public int WalkFPS;
    public Sprite[] AttackSprites;
    public int AttackFPS;
}
