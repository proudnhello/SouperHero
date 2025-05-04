// portions of this file were generated using GitHub Copilot
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Vector2 inputDir;
    Rigidbody2D rb;
    PickUpAndThrow pickup;

    private bool _useMouse;
    private Vector2 _previousMousePosition;
    InputAction movementInput;
    public bool charging = false;

    internal float currrentMoveSpeed = 0;
    internal Vector2 currentDirection;

    private bool canDash = true;
    private bool isDashing = false;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] public float dashSpeed = 15f;
    [SerializeField] public float dashCooldown = 1f;

    public static event Action dash;

    // Start is called before the first frame update
    void Awake()
    {
        _previousMousePosition = Input.mousePosition;
        rb = GetComponent<Rigidbody2D>();
        pickup = gameObject.GetComponent<PickUpAndThrow>();
    }

    private void Start()
    {
        PlayerEntityManager.Singleton.input.Player.Dash.started += Dash;
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

        PlayerEntityManager.Singleton.input.Player.Dash.started -= Dash;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            return;
        }
        // This code is duplicated in CameraFollower.cs. We should consolidate the two into a singleton.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (((Vector2)Input.mousePosition - _previousMousePosition).sqrMagnitude > 0.01f)
        {
            _useMouse = true;
        }
        else
        {
            _useMouse = false;
        }

        currentDirection = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y).normalized;
        pickup.Direction = currentDirection;

        PlayerEntityManager.Singleton.playerAttackPoint.parent.transform.up = currentDirection; // swivel attack point around player

        inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        _previousMousePosition = Input.mousePosition;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        // If the player is charging, don't allow movement, but still allow the player to rotate
        if (!charging)
        {
            if (PlayerEntityManager.Singleton.GetMoveSpeed() >= 1)
            {
                rb.velocity = inputDir * PlayerEntityManager.Singleton.GetMoveSpeed();
            }
            else
            {
                rb.velocity = inputDir * 1;
            }
            currrentMoveSpeed = rb.velocity.magnitude;
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        StartCoroutine(Dashing());
    }

    public bool IsMoving()
    {
        return rb.velocity.magnitude > .01f;
    }

    public IEnumerator Charge(float chargeTime, float chargeStrength)
    {
        rb.velocity = Vector2.zero;
        charging = true;
        rb.AddForce(currentDirection * chargeStrength, ForceMode2D.Impulse);
        yield return new WaitForSeconds(chargeTime);
        charging = false;
    }

    public IEnumerator Dashing()
    {
        if (canDash)
        {
            canDash = false;
            isDashing = true;
            rb.velocity = inputDir * dashSpeed;
            dash?.Invoke();

            yield return new WaitForSeconds(dashDuration);
            isDashing = false;
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
    }

}
