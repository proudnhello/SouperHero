using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private List<Portal> _incomingPortals;
    private Portal _outgoingPortal;
    [SerializeField]
    private char _blockType;

    private void Awake()
    {
        _incomingPortals = new List<Portal>();
        foreach(Portal p in this.gameObject.GetComponentsInChildren<Portal>()) {
            if (p.accepting())
            {
                _incomingPortals.Add(p);
            } else
            {
                _outgoingPortal = p;
            }
        }
    }

    public List<Portal> getPortals()
    {
        return _incomingPortals;
    }

    public Portal getOutgoingPortal()
    {
        return _outgoingPortal;
    }

    public Bounds GetBounds()
    {
        return RoomGenerator.GetPrefabBounds(this.gameObject); ;
    }
    public char getBlockType() { 
        return _blockType;
    }
}
