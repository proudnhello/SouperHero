using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CollectableObject : Interactable
{
    [Header("Collectable")]   
    [SerializeField] public TextMeshPro toolTipText;

    private Vector2 playerPosition;
    private float collectionSpeed = 6f;
    Collider2D _collider;
    private bool collected = false;
    Collectable _Collectable;
    // Start is called before the first frame update
    public void Init(Collectable col)
    {
        _Collectable = col;
        if(toolTipText != null)
        {
            toolTipText.text = _Collectable.promptText;
        }
        _collider = GetComponent<Collider2D>();
    }

    public void Drop(Vector2 dropPoint)
    {
        transform.position = dropPoint;
        SetHighlighted(false);
    }

    public override void Interact()
    {
        SetInteractable(false);  //Cannot interact multiple times
        _collider.enabled = false;
        StartCoroutine (CollectionAnimation());
    }

    private IEnumerator CollectionAnimation()
    {
        while (Vector2.Distance(transform.position, playerPosition) > 0.01f)
        {
            playerPosition = PlayerEntityManager.Singleton.gameObject.transform.position;
            transform.position = Vector2.MoveTowards(transform.position, playerPosition, collectionSpeed * Time.deltaTime);
            yield return null;
        }
        this.transform.parent.GetComponent<Collectable>().Collect();
    }
}
