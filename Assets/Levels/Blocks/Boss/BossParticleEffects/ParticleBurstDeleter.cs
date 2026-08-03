using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBurstDeleter : MonoBehaviour
{
    ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Play();
    }

    //Waits for the particle system to finish playing then destroys the game object
    void FixedUpdate()
    {
        if (!ps.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
