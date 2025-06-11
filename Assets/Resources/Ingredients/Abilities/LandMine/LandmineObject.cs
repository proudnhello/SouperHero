using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = FinishedSoup.SoupInfliction;

public class LandmineObject : MonoBehaviour
{
    [SerializeField] Explosion explosion;
    [SerializeField] GameObject warningRadius;
    float size = 6f;
    [SerializeField] float cycleTime = 0.5f;

    public void init(float size)
    {
        this.size = size;
        warningRadius.transform.localScale = new Vector3(size, size, size);
    }

    public IEnumerator Detonate(float timeActive, float size, List<Infliction> inflictions)
    {
        float remainingTime = timeActive;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        while (remainingTime > 0.1)
        {
            cycleTime = remainingTime / 6;
            spriteRenderer.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(cycleTime);
            spriteRenderer.color = new Color(1, 0, 0, 1);
            yield return new WaitForSeconds(cycleTime);
            remainingTime -= cycleTime * 2;
        }
        Explosion ex = Instantiate(explosion, transform.position, Quaternion.identity);
        ex.Explode(inflictions, size);
        Destroy(gameObject);
    }

    public IEnumerator Detonate(float timeActive, float size, float damage)
    {
        float remainingTime = timeActive;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        while (remainingTime > 0.1)
        {
            cycleTime = remainingTime / 6;
            spriteRenderer.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(cycleTime);
            spriteRenderer.color = new Color(1, 0, 0, 1);
            yield return new WaitForSeconds(cycleTime);
            remainingTime -= cycleTime * 2;
        }
        Explosion ex = Instantiate(explosion, transform.position, Quaternion.identity);
        ex.Explode((int)damage, size);
        Destroy(gameObject);
    }
}
