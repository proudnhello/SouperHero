using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToPot : MonoBehaviour
{

    [SerializeField] private GameObject ingredientObj;
    [SerializeField] private GameObject potObj;
    private float OffsetX = -50.0f; //Offset to spawn collected ingredient above the potUI
    private float OffsetY = 85.0f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject g = Instantiate(ingredientObj, new Vector2(potObj.transform.position.x + OffsetX, potObj.transform.position.y + OffsetY), Quaternion.identity, this.transform);
        //g.transform.localScale = g.transform.localScale * 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
