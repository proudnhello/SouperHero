using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBounds : MonoBehaviour
{
    Dictionary<Hazard, bool> containedHazards = new Dictionary<Hazard, bool>();
    Entity parent;

    private void Start()
    {
        parent = GetComponentInParent<Entity>();
        if (parent == null)
        {
            Debug.LogError("EntityBounds must be a child of an Entity");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hazard hazard = collision.GetComponent<Hazard>();
        if (hazard != null)
        {
            if (!containedHazards.ContainsKey(hazard))
            {
                containedHazards.Add(hazard, true);
                parent.UpdateBounds(hazard);
            }
        }
    }

    public bool CheckHazard(Hazard hazard)
    {
        return containedHazards.ContainsKey(hazard);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Hazard hazard = collision.GetComponent<Hazard>();
        if (hazard != null)
        {
            if (containedHazards.ContainsKey(hazard))
            {
                containedHazards.Remove(hazard);
                parent.UpdateBounds(hazard);
            }
        }
    }
}
