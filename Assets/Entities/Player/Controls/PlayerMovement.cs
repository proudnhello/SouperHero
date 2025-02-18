using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    float horizontal;
    float vertical;
    Rigidbody2D rb;
    private bool _useMouse;
    private Vector2 _previousMousePosition;
    InputAction movementInput;

    internal float currrentMoveSpeed = 0;
    internal Vector2 currentDirection;

    // Start is called before the first frame update
    void Awake()
    {
        _previousMousePosition = Input.mousePosition;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        movementInput = PlayerEntityManager.Singleton.input.Player.Movement;
    }

    void OnEnable()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    void OnDisable()
    {
        rb.bodyType = RigidbodyType2D.Kinematic; // Prevent physics interactions
        rb.velocity = Vector2.zero; // Stop movement
        rb.angularVelocity = 0f; // Stop rotation

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false; // Completely disable collisions
        }
    }



    // Update is called once per frame
    void Update()
    {
        // This code is duplicated in CameraFollower.cs. We should consolidate the two into a singleton.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (((Vector2)Input.mousePosition - _previousMousePosition).sqrMagnitude > 0.01f)
        {
            _useMouse = true;
        } else
        {
            _useMouse = false;
        }

        currentDirection = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);

        PlayerEntityManager.Singleton.playerAttackPoint.parent.transform.up = currentDirection; // swivel attack point around player

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        _previousMousePosition = Input.mousePosition;
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal, vertical).normalized * PlayerEntityManager.Singleton.GetMoveSpeed();
        currrentMoveSpeed = rb.velocity.magnitude;
    }


}
