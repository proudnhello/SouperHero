using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public GameObject _buttonBackground;
    public RectTransform _buttonBackgroundRectTransform;
    public Vector2 _targetScale;
    public float _animTime = 0.5f;

    void Start()
    {
        _targetScale = _buttonBackground.transform.localScale;
        _buttonBackground.transform.localScale = Vector3.zero;
        _buttonBackgroundRectTransform = _buttonBackground.GetComponent<RectTransform>();
    }
}
