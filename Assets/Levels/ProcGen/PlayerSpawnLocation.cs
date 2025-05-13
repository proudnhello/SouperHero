using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnLocation : MonoBehaviour
{

    public void SetPlayerPosition()
    {
        StartCoroutine(SetPosition());
    }

    IEnumerator SetPosition()
    {
        yield return null;
        PlayerEntityManager.Singleton.transform.position = transform.position;
    }
}
