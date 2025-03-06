using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Campfire : Interactable
{
    [Header("Campfire")]
    [SerializeField] float playerDistanceToCancelCooking = 3f;
    bool isCooking;

    // Start is called before the first frame update
    private void Start()
    {
        SetInteractable(true);
        SetHighlighted(false);
    }

    public override void Interact()
    {
        if (CanInteract() && !isCooking)
        {       
            Cook();
        } 
    }

    private void Cook()
    {
        CookingManager.Singleton.EnterCooking(this);
        isCooking = true;
        StartCoroutine(TrackPlayerDistance());
    }

    public void StopCooking()
    {
        isCooking = false;
    }

    IEnumerator TrackPlayerDistance()
    {
        while (isCooking)
        {
            //Debug.Log(Vector2.Distance(PlayerEntityManager.Singleton.transform.position, transform.position));
            if (Vector2.Distance(PlayerEntityManager.Singleton.transform.position, transform.position) > playerDistanceToCancelCooking)
            {
                CookingManager.Singleton.ExitCooking();
            }
            yield return null;
        }
    }

    public Vector3 GetCanvasPosition()
    {
        return transform.GetChild(0).position;
    }
}
