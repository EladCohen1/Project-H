using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Reveals OnJump function, that should be attached to the jump input through the input system or used manually whenever the player jumps (given the input context)
// Reveals bools allowing wall hops (requires PlayerWallHop util) and dash jump (Requires PlayerDash Util)

// Reveals JumpImpulse and jumpButtonGracePeriod as public variables

// Requires the touchingDirections util
// Binds to animator trigger "Jump" that will lead to jump animation whenever OnJump is triggered
// Requires use of AnimationStrings for animator to work 

// Requires "PlayerUtils" util

// Dash:
// Requires PlayerDash util for dash jump to work

// WallHop:
// Requires PlayerWallHop util for wallhop to work

// Anim:
// Requires Animator using the AnimationStrings (Specific names) to trigger animations

// Grace:
// Built in Grace period

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerJump : MonoBehaviour
{
    [Header("Components")]
    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    PlayerWallHop wallHopController;
    PlayerDash dashController;
    Animator anim;
    PlayerUtils utils;

    [Header("Bools")]
    public bool canWallhop;
    public bool canDashJump;

    [Header("Stats")]
    public float jumpImpulse = 8f;
    public float jumpButtonGracePeriod = 0.1f;
    public float dashJumpImpulse = 20f;

    [Header("Util Variables")]
    public float? lastGroundedTime = null;
    private bool _isJumping = false; // tracks if the player is on the way up on a manual jump

    public bool IsJumping
    {
        get { return _isJumping; }
        set
        {
            _isJumping = value;
            if (value && anim != null && GeneralUtils.HasParameter(AnimationStrings.Jump, anim))
                anim.SetTrigger(AnimationStrings.Jump);
        }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        wallHopController = GetComponent<PlayerWallHop>();
        dashController = GetComponent<PlayerDash>();
        utils = GetComponent<PlayerUtils>();
    }

    void FixedUpdate()
    {
        if (touchingDirections.IsGrounded)
        {
            lastGroundedTime = Time.time;
        }
        if (IsJumping && rb.velocity.y <= 0)
        {
            IsJumping = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (Time.time - lastGroundedTime <= jumpButtonGracePeriod && !dashController._isDashing) // normal jump
            {
                _isJumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
                lastGroundedTime = null;
            }
            else if (canWallhop && wallHopController._canWallHop) // wall jump
            {
                wallHopController.WallHop();
            }
            else if (canDashJump && dashController._isDashing && Time.time - lastGroundedTime <= jumpButtonGracePeriod) // Dash jump
            {
                dashController._isDashJumping = true;
                rb.velocity = new Vector2(utils.playerdirectionAsNumber * dashJumpImpulse, jumpImpulse);
            }
        }
    }
}
