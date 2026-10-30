using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuAnim : MonoBehaviour
{
    private bool facingRight = false;
    [SerializeField] Animator playerAnimator;

    void Update()
    {
        if(Camera.main.ScreenToWorldPoint(Input.mousePosition).x < transform.position.x && facingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = false;
        } else if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x > transform.position.x && !facingRight)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            playerAnimator.SetTrigger("Left Click");
        }
    }


    public void ResetTrigger()
    {
        playerAnimator.ResetTrigger("Left Click");
    }
}
