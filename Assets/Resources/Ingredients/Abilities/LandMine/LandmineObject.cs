using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;
using InflictionStat = FinishedSoup.SoupInflictionStat;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;

public class LandmineObject : MonoBehaviour
{
    [SerializeField] Explosion explosion;
    [SerializeField] GameObject warningRadius;
    float size = 6f;
    [SerializeField] float cycleTime = 0.5f;
    [SerializeField] List<InflictionFlavor> PreBuiltFlavors;
    List<InflictionStat> ExplosionInflictions = new();

    public void init(float size, List<InflictionStat> inflictions = null)
    {
        this.size = size;
        warningRadius.transform.localScale = new Vector3(size, size, size);
        if (inflictions == null)
        {
            foreach (var flavor in PreBuiltFlavors)
            {
                InflictionStat infliction = new(flavor.inflictionType);
                infliction.Add(flavor.amount);
                ExplosionInflictions.Add(infliction);
            }
        }
        else ExplosionInflictions = inflictions;
    }

    public IEnumerator Detonate(float timeActive, float size)
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
        ex.Explode(ExplosionInflictions, size);
        Destroy(gameObject);
    }
}
