using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapDetection : MonoBehaviour
{
    public List<GameObject> minimapRendererList;
    private bool inStart = false;


    void OnTriggerEnter2D(Collider2D collision)
    {

        CircleCollider2D startCircleCollider = collision as CircleCollider2D;
        if (startCircleCollider != null)
        {
            inStart = true;
        }


      

        if (inStart && collision.gameObject.CompareTag("Player"))
        {
            turnOffAll();
        }
    }


    private void turnOffAll()
    {
        for (int i = 0; i < minimapRendererList.Count; i++)
        {
            minimapRendererList[i].SetActive(false);
        }
    }


}
