using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddItemsTEST : MonoBehaviour
{

    public GameObject ingredientObj;
    public GameObject potObj;

    // Start is called before the first frame update
    void Start()
    {
        GameObject g = Instantiate(ingredientObj, new Vector2(potObj.transform.position.x - 50.0f, potObj.transform.position.y + 85.0f), Quaternion.identity, this.transform);
        //g.transform.localScale = g.transform.localScale * 10;

        //Instantiate(ingredientObj);
        //Instantiate(ingredientObj, new Vector2(0,0), Quaternion.identity, potObj.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
