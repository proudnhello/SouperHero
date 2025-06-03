using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Class that is attatched to the inventory slot/button
// Allows for clicking and hovering over a slot

// Utilized this tutorial: https://www.youtube.com/watch?v=-fdG9sG2yk4&t=6s

public class HoverSlot : Selectable
{
    [Header("Hover Settings")]
    private float hoverDelay = 0.5f;
    private bool hoverEnabled = true;

    [Header("Hover Events")]
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    private Coroutine _hoverRoutine;

    private WaitForSeconds _waitTimeHoverDelay;

    private Coroutine _resetRoutine;


    protected new void Reset()
    {
        var imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            imageComponent = gameObject.AddComponent<Image>();
        }

        targetGraphic = imageComponent;
    }

    protected override void Start()
    {
        base.Start();

        _waitTimeHoverDelay = new WaitForSeconds(hoverDelay);
    }

    #region Setup

    public void SetHoverDelay(float hoverDelay)
    {
        this.hoverDelay = hoverDelay;
        _waitTimeHoverDelay = new WaitForSeconds(hoverDelay);
    }

    public void SetHoverEnabled(bool hoverEnabled)
    {
        this.hoverEnabled = hoverEnabled;
    }

    #endregion


    #region Hovering

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!hoverEnabled) { return; }

        base.OnPointerEnter(eventData);

        if (_hoverRoutine != null)
        {
            StopCoroutine(_hoverRoutine);
        }

        _hoverRoutine = StartCoroutine(OnHoverDelay());
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!hoverEnabled) { return; }

        base.OnPointerExit(eventData);

        OnHoverExit?.Invoke();

        if (_hoverRoutine != null)
        {
            StopCoroutine(_hoverRoutine);
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (!hoverEnabled) { return; }

        if (_hoverRoutine != null)
        {
            StopCoroutine(_hoverRoutine);
        }
        _hoverRoutine = StartCoroutine(OnHoverDelay());
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (!hoverEnabled) { return; }

        if (_hoverRoutine != null)
        {
            StopCoroutine(_hoverRoutine);
        }
        OnHoverExit?.Invoke();
    }

    private IEnumerator OnHoverDelay()
    {
        yield return _waitTimeHoverDelay;
        OnHoverEnter?.Invoke();
    }

    #endregion

}