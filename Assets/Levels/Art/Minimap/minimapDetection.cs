using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapDetection : MonoBehaviour
{
    public List<GameObject> minimapRendererList;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
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
