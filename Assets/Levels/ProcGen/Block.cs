using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private string _blockType = "null";
    public bool connected = false;

    public bool west;
    public bool east;
    public bool south;
    public bool north;

    public string BlockType()
    {
        return _blockType;
    }

    public void setDirections(bool north, bool south, bool east, bool west)
    {
        this.north = north;
        this.south = south;
        this.east = east;
        this.west = west;
    }
}
