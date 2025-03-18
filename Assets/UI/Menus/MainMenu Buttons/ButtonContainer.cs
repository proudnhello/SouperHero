using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// Container class for selecting between menu options
// Using DoTween
public class ButtonContainer : MonoBehaviour
{
    public List<ButtonScript> buttons;

    // Store the original positions of the button objects so that you can bring them back to where they were
    // ChatGPT had a good idea to make it generalized into a dictionary. Thank you my dearest AI overlord \\ >.< //
    private Dictionary<ButtonScript, float> originalPositions = new Dictionary<ButtonScript, float>();
    public float _animTime = 0.5f;
    public float _scaleFactor = 0.8f;
    public float _moveDistance = 20.0f;
    public float _xScaleFactor = 0.85f;
    private ButtonScript _currentButton;

    // Store all of the original positions of the buttons, and add event triggers to each one to check if its being hovered or not
    // TODO: find a way to run the onButtonHover on the start button, probably have to change the script execution order in build
    void Start()
    {
        _currentButton = buttons[0];
        foreach (ButtonScript button in buttons)
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

            AddEvent(trigger, EventTriggerType.PointerEnter, () => OnButtonHover(button));
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
                    button._buttonBackgroundRectTransform.DOScale(button._targetScale, button._animTime).SetEase(Ease.OutQuint);

                    // Button's spoon here :)
                    button._spoon1.SetActive(true);

                    //rect.DOLocalMoveX(originalPositions[button] * _xScaleFactor, _animTime).SetEase(Ease.OutQuad);
                }
                else
                {
                    button._buttonBackgroundRectTransform.DOKill();
                    button._buttonBackgroundRectTransform.transform.localScale = Vector3.zero;

                    // Button's spoon gone :(
                    button._spoon1.SetActive(false);

                    //rect.DOLocalMoveX(originalPositions[button], _animTime).SetEase(Ease.OutQuad);
                }
            }
        }
    }
}
