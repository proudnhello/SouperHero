using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSceneTransition : MonoBehaviour
{
    [SerializeField] float animTime;

    [SerializeField] Image blackFade;

    internal bool inProgress;

    public void FadeToBlack()
    {
        inProgress = true;
        var s = DOTween.Sequence();
        s.Append(blackFade.DOColor(new Color(1, 1, 1, 1), animTime).SetEase(Ease.InQuad));
        s.OnComplete(() => { inProgress = false; });
    }
}
