using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int _blockWidth;
    [SerializeField]
    private int _blockHeight;

    [SerializeField]
    private string _blockType = "null";

    public bool west;
    public bool east;
    public bool south;
    public bool north;

    public int BlockWidth()
    {
        return _blockWidth;
    }

    public int BlockHeight()
    {
        return _blockHeight;
    }

    public string BlockType()
    {
        return _blockType;
    }
}
