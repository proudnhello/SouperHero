using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CookingPotButton : MonoBehaviour
{
    bool isInteractable = false;
    [SerializeField] Collider2D _PotCollider;
    SpriteRenderer _SpriteRenderer;
    int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    private void Awake()
    {
        CookingScreen.CookingScreenIsOut += CookingScreenState;
        _SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void CookingScreenState(bool enter)
    {
        if (enter)
        {
            StartCoroutine(HandleInteract());
            PlayerEntityManager.Singleton.input.UI.Click.started += OnClick;
        }
        else
        {
            StopAllCoroutines();
            PlayerEntityManager.Singleton.input.UI.Click.started -= OnClick;
        }
    }

    private void OnDisable()
    {
        PlayerEntityManager.Singleton.input.UI.Click.started -= OnClick;
    }

    void OnClick(InputAction.CallbackContext ctx)
    {
        if (!isInteractable) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!_PotCollider.bounds.IntersectRay(ray)) return;

        CookingScreen.Singleton.CookTheSoup();
    }

    
    IEnumerator HandleInteract()
    {
        while (true)
        {
            isInteractable = false;
            _SpriteRenderer.material.SetFloat(_OutlineThickness, 0);

            if (!CookingScreen.Singleton.SoupIsValid) { yield return null; continue; }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!_PotCollider.bounds.IntersectRay(ray)) { yield return null; continue; }

            _SpriteRenderer.material.SetFloat(_OutlineThickness, 1);
            isInteractable = true;
            yield return null;
        }
    }
}