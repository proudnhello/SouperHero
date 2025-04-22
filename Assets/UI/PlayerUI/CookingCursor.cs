using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingCursor : MonoBehaviour
{
    public Image currentImage;
    public Collectable currentCollectableReference;

    public void switchCursorImageTo(Collectable selectedCollectable, Image newImage)
    {
        currentImage.enabled = true;
        currentImage.sprite = newImage.sprite;
        currentCollectableReference = selectedCollectable;
    }

    public void removeCursorImage()
    {
        currentImage.sprite = null;
        currentImage.enabled = false;
        currentCollectableReference = null;
    }

    private void OnDisable()
    {
        if (currentImage.sprite != null)
        {
            currentImage.sprite = null;
        }
    }

    public void changeToInvalidColor()
    {
        currentImage.color = new Color(255, 0, 0, 255);
    }

    public void resetImageColor()
    {
        currentImage.color = new Color(255, 255, 255, 255);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }
}
