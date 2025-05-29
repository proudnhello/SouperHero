using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Campfire : Interactable
{
    [Header("Campfire")]
    [SerializeField] float playerDistanceToCancelCooking = 3f;
    public Vector3 CameraOffset = new Vector3(0, 30, 0);
    bool isPrepping;

    // Start is called before the first frame update
    private void Start()
    {
        SetInteractable(true);
        SetHighlighted(false);
    }

    public override void Interact()
    {
        if (CanInteract() && !isPrepping)
        {       
            Prep();
        }
        RunStateManager.Singleton.SaveRunState();
    }

    private void Prep()
    {
        CookingScreen.Singleton.EnterCooking(this);
        isPrepping = true;
        StartCoroutine(TrackPlayerDistance());
    }

    public void StopPrepping()
    {
        isPrepping = false;
    }

    IEnumerator TrackPlayerDistance()
    {
        while (isPrepping)
        {
            if (Vector2.Distance(PlayerEntityManager.Singleton.transform.position, transform.position) > playerDistanceToCancelCooking)
            {
                CookingScreen.Singleton.ExitCooking();
            }
            yield return null;
        }
    }

    public Vector3 GetCanvasPosition()
    {
        return transform.GetChild(0).position;
    }
}
