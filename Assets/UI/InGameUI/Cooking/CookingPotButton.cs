using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CookingPotButton : MonoBehaviour
{
    bool isInteractable = false;
    [SerializeField] Collider2D _PotCollider;
    [SerializeField] GameObject CookPrompt;
    SpriteRenderer _SpriteRenderer;
    int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    private void Awake()
    {       
        _SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        CookingScreen.CookingScreenIsOut += CookingScreenState;
        CookPrompt.SetActive(false);
        _SpriteRenderer.material.SetFloat(_OutlineThickness, 0);
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
        CookingScreen.CookingScreenIsOut -= CookingScreenState;
    }

    void OnClick(InputAction.CallbackContext ctx)
    {
        if (!isInteractable) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!_PotCollider.bounds.IntersectRay(ray)) return;

        CookPrompt.SetActive(false);
        CookingScreen.Singleton.CookTheSoup();
    }

    
    IEnumerator HandleInteract()
    {
        CookPrompt.SetActive(false);
        while (true)
        {
            isInteractable = false;
            CookPrompt.SetActive(false);
            _SpriteRenderer.material.SetFloat(_OutlineThickness, 0);

            if (!CookingScreen.Singleton.SoupIsValid) { yield return null; continue; }
            CookPrompt.SetActive(true);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!_PotCollider.bounds.IntersectRay(ray)) { yield return null; continue; }

            _SpriteRenderer.material.SetFloat(_OutlineThickness, 1);
            isInteractable = true;
            yield return null;
        }
    }
}