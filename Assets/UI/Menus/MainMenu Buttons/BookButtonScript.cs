using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

// Button class to store all of the objects under a button, since I'm not sure how to use UI prefabs :\
// It is what is it or something
public class BookButtonScript : MonoBehaviour, IPointerClickHandler
{
    public RectTransform rectTransform;
    public VertexJitter jitter;

    [SerializeField]
    public UnityEvent onClickEvent;

    // Set all member fields

    float baseScale = 0;
    void Start()
    {
        baseScale = rectTransform.localScale.x;
        print(rectTransform);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // On click, disable all animations and invoke the function callback
        rectTransform.DOKill();
        onClickEvent?.Invoke();
    }


    Tween scaleTween;
    public void OnHover(float _animTime, float _scaleFactor)
    {
        scaleTween = rectTransform.DOScale(_scaleFactor, _animTime).SetEase(Ease.OutCubic);
        jitter.StartJitter();
    }

    public void OnUnhover(float _animTime)
    {
        if (scaleTween == null) return;
        float elapsed = scaleTween.Elapsed() == 0 ? _animTime : scaleTween.Elapsed();
        scaleTween.Kill();
        rectTransform.DOScale(baseScale, elapsed).SetEase(Ease.OutCubic);
        jitter.EndJitter();
    }
}
