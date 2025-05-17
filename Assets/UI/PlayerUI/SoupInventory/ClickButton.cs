using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Class that is attatched to the inventory slot/button
// Allows for clicking and hovering over a slot

// Utilized this tutorial: https://www.youtube.com/watch?v=-fdG9sG2yk4&t=6s

public class ClickButton : Selectable, IPointerClickHandler, ISubmitHandler
{
    [Header("Click Settings")]
    private float singleClickDelay = 0.25f;
    private float hoverDelay = 0.5f;
    private bool hoverEnabled = true;

    [Header("ClickEvents")]
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    private float _lackClickTime = 0f;
    private Coroutine _clickCoroutine;
    private Coroutine _hoverRoutine;

    private WaitForSeconds _waitTimeSingleClickDelay;
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

        _waitTimeSingleClickDelay = new WaitForSeconds(singleClickDelay);
        _waitTimeHoverDelay = new WaitForSeconds(hoverDelay);
    }

    #region Setup

    public void SetClickDelays(float singleClickDelay)
    {
        this.singleClickDelay = singleClickDelay;
        _waitTimeSingleClickDelay = new WaitForSeconds(singleClickDelay);
    }

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


    #region Clicking

    public void OnPointerClick(PointerEventData eventData)
    {
        HandlingInput();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        DoStateTransition(SelectionState.Pressed, true);

        HandlingInput();

        if(_resetRoutine != null)
        {
            StopCoroutine(OnFinishSubmit());
        }

        _resetRoutine = StartCoroutine(OnFinishSubmit());
    }

    private void HandlingInput()
    {
        if(!interactable) { return; }

        float timeSinceLastClick = Time.time - _lackClickTime;
        _lackClickTime = Time.time;
        HandleSingleClick();
    }

    private void HandleSingleClick()
    {
        if(_clickCoroutine != null)
        {
            StopCoroutine(_clickCoroutine);
        }

        _clickCoroutine = StartCoroutine(SingleClickDelay());
    }

    private IEnumerator SingleClickDelay()
    {
        yield return _waitTimeSingleClickDelay;
        OnClick?.Invoke();
    }

    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        DoStateTransition(currentSelectionState, false);
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