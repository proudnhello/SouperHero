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
    public static event System.Action Collected;
    protected const float dropLifetime = 30f; //How long object will last once dropped


    public void Init(Collectable col)
    {
        _Collectable = col;
        _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
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
        Collected?.Invoke();
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

    [SerializeField] float BOUNCE_HEIGHT = 2f;
    [SerializeField] float BOUNCE_DISTANCE = .1f;
    [SerializeField] float GRAVITY = -.98f;
    public IEnumerator Bounce(Vector2 dir, float knockback)
    {

        float knockAdjustHeight = map(knockback, 0f, 15f, .5f, 1f);
        float knockAdjustLength = map(knockback, 0f, 15f, .1f, 1f);

        for (int bounce = 3; bounce > 0; bounce--)
        {
            float heightVelocity = BOUNCE_HEIGHT * (2 << bounce) * knockAdjustHeight;
            float lengthVelocity = BOUNCE_DISTANCE * (2 << bounce) * knockAdjustLength;

            float currTopPoint = .0001f;
            Vector2 currBottomPoint = transform.position;
            while (currTopPoint > 0)
            {
                currTopPoint += heightVelocity;
                currBottomPoint += dir * lengthVelocity;

                transform.position = new Vector2(currBottomPoint.x, currBottomPoint.y + currTopPoint);
                heightVelocity += GRAVITY;
                yield return new WaitForFixedUpdate();
            }

            transform.position = new Vector2(currBottomPoint.x, currBottomPoint.y);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.01f);
            foreach (Collider2D collider in colliders)
            {
                // If the collectable were to be spawned in a pit hazard, don't spawn it
                if (collider.gameObject.CompareTag("PitHazard"))
                {
                    Destroy(this.transform.parent.gameObject);
                    break;
                }
            }
        }
    }

    private static float map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return Mathf.Clamp((value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow, toLow, toHigh);
    }
}
