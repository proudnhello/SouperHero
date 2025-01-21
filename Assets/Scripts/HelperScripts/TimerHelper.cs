using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Timer helper class for deactivating objects in the pool. Singleton manager available to any scriptable objects to give coroutine funcitonality.
public class TimerHelper : MonoBehaviour
{
    public static TimerHelper instance { get; private set; }

    // On awake, create the instance and assign it to the member field.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Specifically for ProjectileScriptableObject, to deactivate after a delay instead of destroying them.
    // Calls coroutine.
    public void DeactivateAfterDelay(GameObject target, float delay)
    {
        StartCoroutine(DeactivateCoroutine(target, delay));
    }

    // Coroutine that disables object after delay provided
    private IEnumerator DeactivateCoroutine(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}
