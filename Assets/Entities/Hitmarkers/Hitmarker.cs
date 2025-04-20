// portions of this file were generated using GitHub Copilot
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hitmarker : MonoBehaviour
{
    public float speed;
    public float lifetime = 0.5f;
    float timeLeft;
    // Shoot out in a random direction
    void Start()
    {
        timeLeft = lifetime;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Random.insideUnitCircle.normalized * speed;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        float alpha = timeLeft / lifetime;
        Color color = GetComponentInChildren<TextMeshPro>().color;
        color.a = alpha;
        GetComponentInChildren<TextMeshPro>().color = color;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
