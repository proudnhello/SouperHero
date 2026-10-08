using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableParticleCreator : MonoBehaviour
{
    [SerializeField] ParticleSystem particles;
    [SerializeField] int emitCount = 12;
    // Start is called before the first frame update
    public void Emit()
    {
        transform.parent = null;
        particles.Emit(emitCount);
        StartCoroutine(Wait());

        IEnumerator Wait()
        {
            Debug.Log("trigger");
            while (particles.IsAlive())
            {
                yield return null;
            }
            Destroy(gameObject);
        }

    }
}
