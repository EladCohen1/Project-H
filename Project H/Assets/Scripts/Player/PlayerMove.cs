using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Allows implementation of MOD button to walk instead of run, use PlayerUtils for this feature

public class PlayerMove : MonoBehaviour
{
    [Header("Componenets")]
    Rigidbody2D rb;
    Animator anim;
    TouchingDirections touchingDirections;
    PlayerDash dashController;
    PlayerUtils utils;


    [Header("Stats")]

    [Header("Grounded Movement Stats")]
    public float walkSpeed = 4f;
    public float runSpeed = 10f;
    public float acceleration = 3f;
    public float deceleration = 3.5f;
    public float velPower = 1.3f;
    public float frictionAmount = 0.1f;


    [Header("Air Movement Stats")]
    public float airAcceleration = 1f;
    public float maxAirSpeedByInputAcceleration = 7f;
    public float airAccelerationMax = 1f;
    public float airAccelerationMin = 0.1f;


    [Header("State")]
    [SerializeField]
    private bool _isMoving = false;

    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; anim.SetBool(AnimationStrings.IsMoving, value); } }

    [Header("Inputs")]
    public Vector2 moveInput;

    public float CurrentXMoveSpeed
    {
        get
        {
            // Stop movement to not go through a wall
            if (!touchingDirections.IsOnWall)
            {
                if (IsMoving && utils.IsModPressed)
                {
                    return walkSpeed;
                }
                else if (IsMoving)
                {
                    return runSpeed;
                }
            }
            return 0;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        utils = GetComponent<PlayerUtils>();
        touchingDirections = GetComponent<TouchingDirections>();
        dashController = GetComponent<PlayerDash>();
    }

    void FixedUpdate()
    {
        if (utils._canMove && !dashController._isDashing)
        {
            HandlePlayerRotation();
            HandleMovement();
        }
    }

    // Utils
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

    // Handlers
    private void HandlePlayerRotation()
    {
        if (moveInput.x > 0)
        {
            utils.IsFacingRight = true;
        }
        else if (moveInput.x < 0)
        {
            utils.IsFacingRight = false;
        }
    }
    private void HandleMovement()
    {
        if (touchingDirections.IsGrounded)
        {
            HandleGroundedMovement();
        }
        else
        {
            if (touchingDirections.IsOnWall || touchingDirections.IsOnWallFromBehind)
            {
                HandleAirWallCollision();
            }
            else
            {
                HandleAirborneMovement();
            }
        }
    }
    private void HandleGroundedMovement()
    {
        float targetSpeed = GetXMovementInputDirection() * CurrentXMoveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);
        if (targetSpeed == 0)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        // if (accelRate == deceleration)
        // {
        //     add deceleration animation
        // }
        // if(Mathf.Abs(speedDif) > Mathf.Abs(targetSpeed)){
        //     means turning, add turning animation
        // }
    }
    private void HandleAirborneMovement()
    {
        // if didn't reach max air velocity by input acceleration or trying to accelerate against current speed
        if (Mathf.Abs(rb.velocity.x) < maxAirSpeedByInputAcceleration || (GetXMovementInputDirection() * rb.velocity.x < 0))
        {
            rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, rb.velocity.y);
        }
    }
    private void HandleAirWallCollision()
    {
        if (touchingDirections.IsOnWall)
        {
            if (utils.playerdirectionAsNumber * touchingDirections.onWallXDirection > 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, rb.velocity.y);
            }
        }
        else if (touchingDirections.IsOnWallFromBehind) // allowing movement away from the wall but not into the wall
        {
            if (utils.playerdirectionAsNumber * touchingDirections.onWallFromBehindXDirection > 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, rb.velocity.y);
            }
        }
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (utils._canMove)
        {
            IsMoving = moveInput.x != 0;
        }
    }
}
