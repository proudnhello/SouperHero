using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// Container class for selecting between menu options
// Using DoTween
public class BookButtonContainer : MonoBehaviour
{
    public List<BookButtonScript> buttons;

    // Store the original positions of the button objects so that you can bring them back to where they were
    // ChatGPT had a good idea to make it generalized into a dictionary. Thank you my dearest AI overlord \\ >.< //
    private Dictionary<BookButtonScript, float> originalPositions = new Dictionary<BookButtonScript, float>();
    public float _animTime = 0.5f;
    public float _scaleFactor = 0.8f;
    public float _moveDistance = 20.0f;
    public float _xScaleFactor = 0.85f;
    private BookButtonScript _currentButton;

    // Store all of the original positions of the buttons, and add event triggers to each one to check if its being hovered or not
    // TODO: find a way to run the onButtonHover on the start button, probably have to change the script execution order in build
    void Start()
    {
        _currentButton = buttons[0];
        foreach (BookButtonScript button in buttons)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Only need to store X position because thats whats moving.
                originalPositions[button] = rect.localPosition.x;
            }

            // Connecting event trigger to hovered event
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, () => OnButtonHover(button, true));
            AddEvent(trigger, EventTriggerType.PointerExit, () => OnButtonHover(button, false));

        }
    }

    public void Enable()
    {
        foreach (var but in buttons)
        {
            if (but.FullyDisabled) continue;
            but.enabled = true;
            if (but.isHovering) but.OnHover(_animTime, _scaleFactor);
        }
    }
    public void Disable()
    {
        foreach (var but in buttons)
        {
            but.OnUnhover(_animTime);
            but.enabled = false;
        }
    }

    // Wrapper function to take care of making the event and attaching it to a callback function
    // I <3 ChatGPT (only sometimes)
    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => callback());
        trigger.triggers.Add(entry);
    }

    // Function to tween the position of a button when it is hovered. Also makes the other ones reset their positions.
    // Kills the animation tween and sets its position. Could just animate it back, but who tf cares realisitcally
    private void OnButtonHover(BookButtonScript _button, bool hoverOn)
    {
        if (hoverOn)
        {
            _button.OnHover(_animTime, _scaleFactor);
        } 
        else
        {
            _button.OnUnhover(_animTime);
        }
    }
}
