using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CollectableObject : Interactable
{
    [Header("Collectable")]   

    private Vector2 playerPosition;
    private float collectionSpeed = 6f;
    Collider2D _collider;
    Collectable _Collectable;
    protected const float dropLifetime = 30f; //How long object will last once dropped

    public void Init(Collectable col)
    {
        _Collectable = col;
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        StartCoroutine(Fade());
    }

    public void Drop(Vector2 dropPoint)
    {
        transform.position = dropPoint;
        SetHighlighted(false);
    }

    public override void Interact()
    {
        SetInteractable(false);  //Cannot interact multiple times
        SetHighlighted(false);
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
    private IEnumerator Fade()
    {
        float timeProgressed = dropLifetime;
        Color normalColor = this.gameObject.GetComponent<SpriteRenderer>().color;

        while (timeProgressed > 0)
        {
            normalColor.a = timeProgressed / (dropLifetime - 20f);
            this.gameObject.GetComponent<SpriteRenderer>().color = normalColor;
            timeProgressed -= Time.deltaTime;
            yield return null;
        }

        Destroy(this.transform.parent.gameObject);
    }
}
