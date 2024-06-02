using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airAcceleration = 0.1f;
    public float maxAirSpeedByInputAcceleration = 2f;
    public float jumpImpulse = 10f;
    public float dashImpulse = 8f;
    public float CurrentXMoveSpeed
    {
        get
        {
            // Stop movement to not go through a wall
            if (!touchingDirections.IsOnWall)
            {
                if (IsMoving && IsModPressed)
                {
                    return runSpeed;
                }
                else if (IsMoving)
                {
                    return walkSpeed;
                }
            }
            return 0;
        }
    }

    [Header("Components")]
    Rigidbody2D rb;
    Animator anim;
    TouchingDirections touchingDirections;

    [Header("Inputs")]
    private Vector2 moveInput;

    // Public status bools
    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; anim.SetBool(AnimationStrings.IsMoving, value); } }
    public bool IsModPressed { get { return _isModPressed; } set { _isModPressed = value; anim.SetBool(AnimationStrings.IsModPressed, value); } }
    public bool IsCombat { get { return _isCombat; } set { _isCombat = value; anim.SetBool(AnimationStrings.IsCombat, value); } }

    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
                _isFacingRight = value;
            }
        }
    }

    [Header("Status Bools")]
    // Private status bools
    [SerializeField]
    private bool _isMoving = false;
    [SerializeField]
    private bool _isModPressed = false;
    [SerializeField]
    private bool _isFacingRight = true;
    [SerializeField]
    private bool _isCombat = false;
    [SerializeField]
    private bool _didDash = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
    }

    void FixedUpdate()
    {
        HandlePlayerRotation();
        anim.SetFloat(AnimationStrings.YVelocity, rb.velocity.y);
        HandleMovement();
    }

    private float GetXMovementInputDirection()
    {
        float movement = 0;
        if (moveInput.x > 0)
        {
            movement = 1;
        }
        else if (moveInput.x < 0)
        {
            movement = -1;
        }
        return movement;
    }
    private void HandlePlayerRotation()
    {
        if (moveInput.x > 0)
        {
            IsFacingRight = true;
        }
        else if (moveInput.x < 0)
        {
            IsFacingRight = false;
        }
    }
    private void HandleMovement()
    {
        if (touchingDirections.IsGrounded)
        {
            HandleGroundedMovement();
            _didDash = false;
        }
        else
        {
            HandleAirborneMovement();
        }
    }
    private void HandleGroundedMovement()
    {
        // Stop movement to prevent going through the ground
        if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(GetXMovementInputDirection() * CurrentXMoveSpeed, 0);
        }
        else
        {
            rb.velocity = new Vector2(GetXMovementInputDirection() * CurrentXMoveSpeed, rb.velocity.y);
        }
    }
    private void HandleAirborneMovement()
    {
        // if didn't reach max air velocity by input acceleration or trying to accelerate against current speed
        if ((Mathf.Abs(rb.velocity.x) < maxAirSpeedByInputAcceleration || (GetXMovementInputDirection() * rb.velocity.x < 0)) &&
        !touchingDirections.IsOnWall)
        {
            // stop movement into a wall from behind
            if (touchingDirections.IsOnWallFromBehind)
            {

            }
            // stop upward movement when touching a celling
            if (!touchingDirections.IsOnCelling)
            {
                rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, 0);
            }
        }
        else if (touchingDirections.IsOnWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput.x != 0;
    }
    public void OnMod(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsModPressed = true;
        }
        else if (context.canceled)
        {
            IsModPressed = false;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (touchingDirections.IsGrounded)
            {
                anim.SetTrigger(AnimationStrings.Jump);
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
            }
            else if (!_didDash)
            {
                float facingDirection = IsFacingRight ? 1 : -1;
                rb.velocity = new Vector2(rb.velocity.x + dashImpulse * facingDirection, rb.velocity.y);
                _didDash = true;
            }
        }
    }
}
