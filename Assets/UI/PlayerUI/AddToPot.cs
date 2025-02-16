using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lo: This class contains the contents of the player's pot
public class AddToPot : MonoBehaviour
{

    //[SerializeField] private GameObject ingredient; //Testing
    [SerializeField] private GameObject pot; //TODO: Replace this
    public static AddToPot Singleton;
    private float OffsetX = -50.0f; //Offset to spawn collected ingredient above the potUI
    private float OffsetY = 85.0f;

    private void Awake()
    {
        if(Singleton != null)
        {
            Destroy(this);
            return;
        }
        Singleton = this;
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void AddIngredient(GameObject ingredient)
    {
        //GameObject gameObj = Instantiate(ingredient, new Vector2(pot.transform.position.x + OffsetX, pot.transform.position.y + OffsetY), Quaternion.identity, this.transform);
        //TODO: Set parent of leek to pot
        ingredient.transform.SetParent(this.transform);
        ingredient.transform.position = new Vector2(pot.transform.position.x + OffsetX, pot.transform.position.y + OffsetY);
    }

    public void RemoveIngredient()
    {

    }
}
