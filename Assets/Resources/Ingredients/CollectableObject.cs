using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

    float BOUNCE_HEIGHT = 0.02f;
    float BOUNCE_DISTANCE = 0.03f;
    float GRAVITY = -0.022f;
    float WALL_CHECK_DIST = 0.1f;
    public IEnumerator Bounce(Vector2 dir, float knockback, int bounces = 3, float startOffset = -1, float groundOffsetY = 0)
    {

        float knockAdjustHeight = map(knockback, 0f, 15f, .375f, .75f);
        float knockAdjustLength = map(knockback, 0f, 15f, .075f, .75f);


        bool hasCollided = false;

        if (startOffset >= 0) bounces+=1;

        Vector2 groundPoint = new Vector2(transform.position.x, transform.position.y + groundOffsetY);

        for (int bounce = bounces; bounce > 0; bounce--)
        {
            float heightVelocity = startOffset >= 0 && bounce == bounces ? 0 : BOUNCE_HEIGHT * (2 << bounce) * knockAdjustHeight;
            float lengthVelocity = startOffset >= 0 ? 0 : BOUNCE_DISTANCE * (2 << bounce) * knockAdjustLength;
            Debug.Log(heightVelocity);
            float currTopPoint = startOffset >= 0 && bounce == bounces ? startOffset : .0001f;
            Vector2 currBottomPoint = groundPoint;
            while (currTopPoint > 0)
            {
                currTopPoint += heightVelocity;
                Debug.Log(bounce + ": " + heightVelocity + " " + currTopPoint);

                if (!hasCollided)
                {
                    RaycastHit2D wall = Physics2D.Raycast(currBottomPoint, dir, WALL_CHECK_DIST, CollisionLayers.Singleton.GetEnvironmentLayer());
                    if (wall.collider != null) { hasCollided = true; bounce = 0; }
                }
                // only move in direction if hasn't collided with wall
                if (!hasCollided) currBottomPoint += dir * lengthVelocity;

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
