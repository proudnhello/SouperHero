using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // Storage class for everything that other classes might need from a portal
    [SerializeField]
    private GameObject _direction;
    private bool _closed = false;
    [SerializeField]
    private bool _accepting;

    public float getDirectionAsAngle()
    {
        Vector2 unitDirection = ((Vector2) (_direction.transform.position - this.gameObject.transform.position)).normalized;
        return Mathf.Atan2(unitDirection.y, unitDirection.x);
    }

    public bool isClosed()
    {
        return _closed;
    }

    public void setClosed(bool isClosed)    
    {
        _closed = isClosed;
    }

    public bool accepting()
    {
        return _accepting;
    }
}
