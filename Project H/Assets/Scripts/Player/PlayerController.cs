using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]

    [Header("Grounded Movement Stats")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float acceleration = 3f;
    public float deceleration = 2f;
    public float velPower = 1.3f;
    public float frictionAmount = 0.1f;

    [Space(10)]
    [Header("Air Movement Stats")]
    public float airAcceleration = 1f;
    public float maxAirSpeedByInputAcceleration = 7f;
    public float airAccelerationMax = 1f;
    public float airAccelerationMin = 0.1f;

    [Space(10)]
    [Header("Dash")]
    public float dashImpulse = 20f;
    public float dashTime = 0.2f;
    public float dashCD = 0.7f;

    [Space(10)]
    [Header("Jump")]
    public float jumpImpulse = 8f;
    public float jumpButtonGracePeriod = 0.1f;


    public float CurrentXMoveSpeed
    {
        get
        {
            // Stop movement to not go through a wall
            if (!touchingDirections.IsOnWall)
            {
                if (IsMoving && IsModPressed)
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
    [Space(10)]
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;
    public TouchingDirections touchingDirections;
    public TrailRenderer tr;
    public PlayerDash dashController;
    public PlayerUtils utils;
    public PlayerWallSlide wallSlideController;
    public PlayerWallHop wallHopController;

    [Space(10)]
    [Header("Inputs")]
    public Vector2 moveInput;

    // Public status bools
    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; anim.SetBool(AnimationStrings.IsMoving, value); } }
    public bool IsModPressed { get { return _isModPressed; } set { _isModPressed = value; anim.SetBool(AnimationStrings.IsModPressed, value); } }
    public bool IsCombat { get { return _isCombat; } set { _isCombat = value; anim.SetBool(AnimationStrings.IsCombat, value); } }
    public bool IsWallSliding { get { return _isWallSliding; } set { _isWallSliding = value; anim.SetBool(AnimationStrings.IsWallSliding, value); } }



    [Header("Status Bools")]
    // Private status bools
    [Header("State")]
    [SerializeField]
    private bool _isMoving = false;

    [SerializeField]
    private bool _isWallSliding = false;
    [SerializeField]
    private bool _isCombat = false;

    [Header("Input")]
    [SerializeField]
    private bool _isModPressed = false;


    [Header("Actions")]

    [SerializeField]
    public bool _isJumping = false; // tracks if the player is on the way up on a manual jump


    [Header("Util Variables")]
    public float? lastGroundedTime = null;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        utils = GetComponent<PlayerUtils>();
        touchingDirections = GetComponent<TouchingDirections>();
        tr = GetComponent<TrailRenderer>();
        dashController = GetComponent<PlayerDash>();
        wallSlideController = GetComponent<PlayerWallSlide>();
        wallHopController = GetComponent<PlayerWallHop>();
    }

    void FixedUpdate()
    {
        #region Anim Update
        anim.SetFloat(AnimationStrings.YVelocity, rb.velocity.y);
        #endregion

        if (touchingDirections.IsGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (dashController._isDashing)
        {
            return;
        }
        #region Movement
        if (utils._canMove)
        {
            HandlePlayerRotation();
            HandleMovement();
        }
        #endregion

        #region Jump
        if (_isJumping && rb.velocity.y <= 0)
        {
            _isJumping = false;
        }
        #endregion
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


    // Events
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (utils._canMove)
        {
            IsMoving = moveInput.x != 0;
        }
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
            if (Time.time - lastGroundedTime <= jumpButtonGracePeriod && !dashController._isDashing) // normal jump
            {
                anim.SetTrigger(AnimationStrings.Jump);
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
                _isJumping = true;
                lastGroundedTime = null;
            }
            else if (wallHopController._canWallHop) // wall jump
            {
                wallHopController.WallHop();
            }
            else if (dashController._isDashing && Time.time - lastGroundedTime <= jumpButtonGracePeriod) // Dash jump
            {
                dashController._isDashJumping = true;
                rb.velocity = new Vector2(utils.playerdirectionAsNumber * dashImpulse, jumpImpulse);
            }
        }
    }
}