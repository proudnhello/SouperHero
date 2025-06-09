using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapDetection : MonoBehaviour
{
    public List<GameObject> minimapRendererList;

    void Start()
    {
        updateRenderList();
        turnOffAll();
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            turnOnAll();
        }
    }

    private void turnOnAll()
    {
        for (int i = 0; i < minimapRendererList.Count; i++)
        {
            minimapRendererList[i].SetActive(true);
        }
    }

    private void turnOffAll()
    {
        for (int i = 0; i < minimapRendererList.Count; i++)
        {
            minimapRendererList[i].SetActive(false);
        }
    }

    private void updateRenderList()
    {
        List<GameObject> tempList = new List<GameObject>();
        for (int i = 0; i < minimapRendererList.Count; i++)
        {
            if (minimapRendererList[i].activeSelf)
            {
                tempList.Add(minimapRendererList[i]);
            }
        }

        minimapRendererList = tempList;
    }

}
