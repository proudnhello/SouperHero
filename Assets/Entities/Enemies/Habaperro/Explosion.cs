using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float timeActive = 2f;
    void Start()
    {
        StartCoroutine(timer());
    }

    IEnumerator timer(){
        yield return new WaitForSeconds(timeActive);
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject))
        {
            try {
                Entity entity = collider.GetComponent<Entity>();
                entity.ModifyHealth(-10);
            }
            catch{
                return;
            }
        }
    }
}