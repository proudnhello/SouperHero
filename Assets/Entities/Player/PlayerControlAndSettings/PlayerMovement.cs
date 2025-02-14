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

    // Start is called before the first frame update
    void Awake()
    {
        _previousMousePosition = Input.mousePosition;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        movementInput = PlayerInputAndAttackManager.Singleton.input.Player.Movement;
    }

    void OnEnable()
    {
        Debug.Log("Player Movement has been enabled.");
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void OnDisable()
    {
        Debug.Log("Player Movement has been disabled.");
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

        Vector2 keyDirection = movementInput.ReadValue<Vector2>().normalized;

        Vector2 direction = _useMouse || keyDirection == Vector2.zero ? new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y) : keyDirection;

        transform.up = direction;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        _previousMousePosition = Input.mousePosition;
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal, vertical).normalized * PlayerEntityManager.Singleton.GetMoveSpeed();
    }


}
