using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UIElements;

public class ButtonContainer : MonoBehaviour
{
    public List<ButtonScript> buttons;

    private Dictionary<ButtonScript, Vector3> originalScales = new Dictionary<ButtonScript, Vector3>();
    public float _animTime = 0.5f;
    public float _scaleFactor = 0.8f;
    public float _moveDistance = 20.0f;
    public float _yScaleFactor = 1.25f;
    private ButtonScript _currentButton;

    void Start()
    {
        _currentButton = buttons[0];
        foreach (ButtonScript button in buttons)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                originalScales[button] = rect.localScale;
            }

            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, () => OnButtonHover(button));
        }
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }

    private void OnButtonHover(ButtonScript hoveredButton)
    {
        _currentButton = hoveredButton;
        foreach (ButtonScript button in buttons)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (button == hoveredButton)
                {
                    rect.DOScale(originalScales[button]* _yScaleFactor, _animTime).SetEase(Ease.OutQuad);
                    button._buttonBackgroundRectTransform.DOScale(button._targetScale, button._animTime).SetEase(Ease.OutQuint);
                }
                else
                {
                    Vector3 scaled = originalScales[button] * _scaleFactor;
                    button._buttonBackgroundRectTransform.DOKill();
                    button._buttonBackgroundRectTransform.transform.localScale = Vector3.zero;
                    rect.DOScale(scaled, _animTime).SetEase(Ease.OutQuad);
                }
            }
        }
    }
}
