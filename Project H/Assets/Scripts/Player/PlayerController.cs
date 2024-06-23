using System.Collections;
using System.Collections.Generic;
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
    public float maxAirSpeedByInputAccelerationMax = 7f;
    public float airAccelerationMin = 0.1f;
    public float maxAirSpeedByInputAccelerationMin = 0.7f;
    public float jumpImpulse = 10f;
    public float wallSlidingXJumpImpulse = 3f;
    public float dashImpulse = 8f;
    public float maxWallSlidingSpeed = 2f;
    public float jumpButtonGracePeriod = 0.2f;
    public float jumpCutMult = 0.5f;
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
    public bool IsWallSliding { get { return _isWallSliding; } set { _isWallSliding = value; anim.SetBool(AnimationStrings.IsWallSliding, value); } }

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
    private bool _didAirDash = false;
    [SerializeField]
    private bool _isWallSliding = false;
    [SerializeField]
    private bool _canMove = true;
    [SerializeField]
    private bool _canWallHop = false;
    [SerializeField]
    private bool _isJumping = false;


    [Header("Util Variables")]
    private Coroutine wallSlideGraceCoroutine = null;
    private Coroutine wallHopGraceCoroutine = null;
    private float? lastGroundedTime = null;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
    }

    void FixedUpdate()
    {
        #region Anim Update
        anim.SetFloat(AnimationStrings.YVelocity, rb.velocity.y);
        #endregion

        #region Wall Slide
        CheckWallSliding();
        CheckWallHop();
        HandleWallSliding();
        #endregion

        #region Movement
        if (touchingDirections.IsGrounded && _didAirDash)
        {
            _didAirDash = false;
        }
        if (_canMove)
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
        if (touchingDirections.IsGrounded)
        {
            lastGroundedTime = Time.time;
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
    private void CheckWallSliding()
    {
        if (!touchingDirections.IsGrounded && touchingDirections.IsOnSlidableWall && rb.velocity.y < 0 && moveInput.y >= 0)
        {
            IsWallSliding = true; // wall sliding
            _canMove = false; // stick to wall
            _didAirDash = false; // reset air dash
        }
        else
        {
            IsWallSliding = false;
            IsMoving = moveInput.x != 0;
            _canMove = true;
        }

        // Checking for detaching from the wall by holding the other direction
        if (IsWallSliding)
        {
            if (moveInput.x * touchingDirections.slidingWallXDirection < 0) // if holding the opposite direction from the wall
            {
                if (wallSlideGraceCoroutine == null)
                {
                    wallSlideGraceCoroutine = StartCoroutine(WallSlidingGrace());
                }
            }
            else
            {
                if (wallSlideGraceCoroutine != null)
                {
                    StopCoroutine(wallSlideGraceCoroutine);
                }
                wallSlideGraceCoroutine = null;
            }
        }
    }
    private void CheckWallHop()
    {
        if (touchingDirections.IsOnSlidableWall || touchingDirections.IsOnSlidableWallFromBehind)
        {
            _canWallHop = true;
            if (wallHopGraceCoroutine != null)
            {
                StopCoroutine(wallHopGraceCoroutine);
            }
            wallHopGraceCoroutine = StartCoroutine(WallHopGrace());
        }
    }

    // Handlers
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
        }
        else
        {
            HandleAirborneMovement();
            HandleAirWallCollision();
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
        // if didn't reach max air velocity by input acceleration or trying to accelerate against current speed or is on wall
        if ((Mathf.Abs(rb.velocity.x) < maxAirSpeedByInputAcceleration || (GetXMovementInputDirection() * rb.velocity.x < 0)) &&
        !(touchingDirections.IsOnWall || touchingDirections.IsOnWallFromBehind))
        {
            rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, rb.velocity.y);
        }
    }
    private void HandleAirWallCollision()
    {
        if (touchingDirections.IsOnWall)
        {
            if (rb.velocity.x * touchingDirections.onWallXDirection > 0)
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
            if (rb.velocity.x * touchingDirections.onWallFromBehindXDirection > 0)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x + GetXMovementInputDirection() * airAcceleration, rb.velocity.y);
            }
        }
    }
    private void HandleWallSliding()
    {
        if (IsWallSliding)
        {
            if (rb.velocity.y <= -maxWallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxWallSlidingSpeed);
            }
        }
    }



    // Coroutines
    IEnumerator WallSlidingGrace()
    {
        yield return new WaitForSeconds(0.25f);
        IsFacingRight = !IsFacingRight;
    }
    IEnumerator WallHopGrace()
    {
        yield return new WaitForSeconds(0.2f);
        _canWallHop = false;
    }
    IEnumerator WallHopLock()
    {
        // slowly regaining air control
        yield return new WaitForSeconds(0.3f);
        float airAccelerationAdd = (airAccelerationMax - airAccelerationMin) / 5;
        float maxAirSpeedByInputAccelerationAdd = (maxAirSpeedByInputAccelerationMax - maxAirSpeedByInputAccelerationMin) / 5;
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.1f);
            airAcceleration += airAccelerationAdd;
            maxAirSpeedByInputAcceleration += maxAirSpeedByInputAccelerationAdd;
        }
        airAcceleration = airAccelerationMax;
        maxAirSpeedByInputAcceleration = maxAirSpeedByInputAccelerationMax;
    }


    // Events
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (_canMove)
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
            if (Time.time - lastGroundedTime <= jumpButtonGracePeriod) // normal jump
            {
                anim.SetTrigger(AnimationStrings.Jump);
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
                _isJumping = true;
                lastGroundedTime = null;
            }
            else if (_canWallHop) // wall jump
            {
                rb.velocity = new Vector2(wallSlidingXJumpImpulse * touchingDirections.slidingWallXDirection * -1, jumpImpulse);
                _didAirDash = false;
                // lock into wall jump for a while after wall jumping to stop wall climbing
                airAcceleration = airAccelerationMin;
                maxAirSpeedByInputAcceleration = maxAirSpeedByInputAccelerationMin;
                StartCoroutine(WallHopLock());
            }
            else if (!_didAirDash && !touchingDirections.IsOnWall) // dash
            {
                float facingDirection = IsFacingRight ? 1 : -1;
                rb.velocity = new Vector2(rb.velocity.x + dashImpulse * facingDirection, rb.velocity.y);
                _didAirDash = true;
            }
        }
    }
}
