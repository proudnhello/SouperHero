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
    private Vector2 _previousMousePosition;
    InputActionReference movementAction;
    private bool isCursorMovement = false;
    private bool cursorButtonHeld = false;
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
    }

    private void Start()
    {
        // PlayerEntityManager.Singleton.input.Player.Dash.started += Dash;
        PlayerKeybinds.Singleton.dash.action.started += Dash;
        isCursorMovement = SettingsManager.Singleton.CursorMovement;
        if (!isCursorMovement)
        {
            movementAction = PlayerKeybinds.Singleton.movement;
        }
        else
        {
            PlayerKeybinds.Singleton.cursorMovement.action.performed += CursorMoveHeld;
            PlayerKeybinds.Singleton.cursorMovement.action.canceled += CursorMoveLetGo;
        }
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

        PlayerKeybinds.Singleton.dash.action.started -= Dash;
        if (isCursorMovement)
        {
            PlayerKeybinds.Singleton.cursorMovement.action.performed -= CursorMoveHeld;
            PlayerKeybinds.Singleton.cursorMovement.action.canceled -= CursorMoveLetGo;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isDashing)
        {
            return;
        }
        // This code is duplicated in CameraFollower.cs. We should consolidate the two into a singleton.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        currentDirection = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y).normalized;

        PlayerEntityManager.Singleton.playerAttackPoint.parent.transform.up = currentDirection; // swivel attack point around player

        if (!isCursorMovement)
        {
            inputDir = movementAction.action.ReadValue<Vector2>();   
        }
        _previousMousePosition = Input.mousePosition;
    }

    void FixedUpdate()
    {
        if(isDashing)
        {
            return;
        }
        // If the player is charging, don't allow movement, but still allow the player to rotate
        if (!charging)
        {
            if (!isCursorMovement)
            {
                if (PlayerEntityManager.Singleton.GetMoveSpeed() >= 1)
                {
                    rb.velocity = inputDir * PlayerEntityManager.Singleton.GetMoveSpeed();
                }
                else
                {
                    rb.velocity = inputDir * 1;
                }
            }
            else if (cursorButtonHeld)
            {
                if (PlayerEntityManager.Singleton.GetMoveSpeed() >= 1)
                {
                    rb.velocity = currentDirection * PlayerEntityManager.Singleton.GetMoveSpeed();
                }
                else
                {
                    rb.velocity = currentDirection * 1;
                }
            }
            currrentMoveSpeed = rb.velocity.magnitude;
        }
    }

    public void CursorMoveHeld(InputAction.CallbackContext ctx)
    {
        cursorButtonHeld = true;
    }

    public void CursorMoveLetGo(InputAction.CallbackContext ctx)
    {
        cursorButtonHeld = false;
        rb.velocity = Vector2.zero;
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
        if (isCursorMovement)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public IEnumerator Dashing()
    {
        if (canDash)
        {
            canDash = false;
            isDashing = true;

            if (!isCursorMovement)
            {
                rb.velocity = inputDir * dashSpeed;
            }
            else
            {
                rb.velocity = currentDirection * dashSpeed;
            }
            dash?.Invoke();

            yield return new WaitForSeconds(dashDuration);
            isDashing = false;
            if (isCursorMovement)
            {
                rb.velocity = Vector2.zero;
            }
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
    }

}
