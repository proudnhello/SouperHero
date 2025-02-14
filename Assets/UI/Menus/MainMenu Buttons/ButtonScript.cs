using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

// Button class to store all of the objects under a button, since I'm not sure how to use UI prefabs :\
// It is what is it or something
public class ButtonScript : MonoBehaviour, IPointerClickHandler
{
    public GameObject _buttonBackground;
    public RectTransform _buttonBackgroundRectTransform;
    public Vector2 _targetScale;
    public float _animTime = 0.5f;
    public GameObject _spoon1;

    [SerializeField]
    public UnityEvent onClickEvent;

    // Set all member fields
    void Start()
    {
        _targetScale = _buttonBackground.transform.localScale;
        _buttonBackground.transform.localScale = Vector3.zero;
        _buttonBackgroundRectTransform = _buttonBackground.GetComponent<RectTransform>();
        _spoon1 = this.transform.GetChild(0).gameObject;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // On click, disable all animations and invoke the function callback
        _buttonBackgroundRectTransform.DOKill();
        this.gameObject.GetComponent<RectTransform>().DOKill();
        onClickEvent?.Invoke();
    }
}
