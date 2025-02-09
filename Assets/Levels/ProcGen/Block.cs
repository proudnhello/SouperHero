using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int _blockWidth;
    [SerializeField]
    private int _blockHeight;

    public int BlockWidth()
    {
        return _blockWidth;
    }

    public int BlockHeight()
    {
        return _blockHeight;
    }


}
