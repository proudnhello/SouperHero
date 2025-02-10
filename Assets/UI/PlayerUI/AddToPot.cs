using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lo: This class contains the contents of the player's pot
public class AddToPot : MonoBehaviour
{

    [SerializeField] private GameObject ingredient; //Testing
    [SerializeField] private GameObject pot; //TODO: replace this
    private float OffsetX = -50.0f; //Offset to spawn collected ingredient above the potUI
    private float OffsetY = 85.0f;

    void Start()
    {
        //GameObject g = Instantiate(ingredient, new Vector2(pot.transform.position.x + OffsetX, pot.transform.position.y + OffsetY), Quaternion.identity, this.transform);
        //g.transform.localScale = g.transform.localScale * 2; //DON'T USE!
    }

    void Update()
    {
        
    }

    public void AddIngredient(Color ingredientColor)
    {
        GameObject g = Instantiate(ingredient, new Vector2(pot.transform.position.x + OffsetX, pot.transform.position.y + OffsetY), Quaternion.identity, this.transform);
        g.GetComponent<UnityEngine.UI.Image>().color = ingredientColor;
    }

    public void RemoveIngredient()
    {

    }
}
