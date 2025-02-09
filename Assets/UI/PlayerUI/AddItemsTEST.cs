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
        Instantiate(ingredientObj, new Vector2(potObj.transform.position.x - 50.0f, potObj.transform.position.y + 85.0f), Quaternion.identity, this.transform);
        //Instantiate(ingredientObj, potObj.transform.position, Quaternion.identity, this.transform);
        //Instantiate(ingredientObj, new Vector2(0,0), Quaternion.identity, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
