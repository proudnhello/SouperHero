using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

// Button class to store all of the objects under a button, since I'm not sure how to use UI prefabs :\
// It is what is it or something
public class BookButtonScript : MonoBehaviour, IPointerClickHandler
{
    public RectTransform rectTransform;
    public VertexJitter jitter;
    public TMP_Text text;

    [SerializeField]
    public UnityEvent onClickEvent;

    public void Disable()
    {
        text.color = new Color(0.3215686f, 0.3215686f, 0.3215686f, 1);
        this.enabled = false;
    }

    // Set all member fields

    float baseScale = 0;
    void Start()
    {
        baseScale = rectTransform.localScale.x;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (!this.enabled) return;
        // On click, disable all animations and invoke the function callback
        rectTransform.DOKill();
        onClickEvent?.Invoke();
    }


    Tween scaleTween;
    internal bool isHovering = false;
    public void OnHover(float _animTime, float _scaleFactor)
    {
        isHovering = true;
        if (!this.enabled) return;
        scaleTween = rectTransform.DOScale(_scaleFactor, _animTime).SetEase(Ease.OutCubic);
        jitter.StartJitter();
    }

    public void OnUnhover(float _animTime)
    {
        if (!isHovering) return;
        isHovering = false;
        if (!this.enabled || scaleTween == null) return;
        float elapsed = scaleTween.Elapsed() == 0 ? _animTime : scaleTween.Elapsed();
        scaleTween.Kill();
        rectTransform.DOScale(baseScale, elapsed).SetEase(Ease.OutCubic);
        jitter.EndJitter();
    }
}
