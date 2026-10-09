using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableParticleCreator : MonoBehaviour
{
    [SerializeField] ParticleSystem hitParticles;
    [SerializeField] int hitEmitCount = 12;
    [SerializeField] ParticleSystem breakParticles;
    [SerializeField] int breakEmitCount = 12;

    public void Hit()
    {
        if (hitParticles != null) hitParticles.Emit(hitEmitCount);
    }

    public void Break()
    {
        if (hitParticles != null) hitParticles.transform.parent = null;
        breakParticles.transform.parent = null;
        breakParticles.Emit(breakEmitCount);
        StartCoroutine(Wait());

        IEnumerator Wait()
        {
            Debug.Log("trigger");
            while (breakParticles.IsAlive())
            {
                yield return null;
            }
            if (hitParticles != null) Destroy(hitParticles.gameObject);
            Destroy(breakParticles.gameObject);
        }

    }
}
