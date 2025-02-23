using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class MapRoom : MonoBehaviour
{
    [SerializeField]
    private int _blockWidth;
    [SerializeField]
    private int _blockHeight;

    // ORDERED FROM TOP LEFT TO BOTTOM RIGHT
    public List<Block> blocks = new List<Block>();

    public int BlockWidth()
    {
        return _blockWidth;
    }

    public int BlockHeight()
    {
        return _blockHeight;
    }

    public Block At(int row, int col)
    {
        return blocks[col * (_blockWidth) + row];
    }

    public void enableAllEnemies()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
}
